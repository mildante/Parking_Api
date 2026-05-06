using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using Parking_Api.Hubs;
using static Parking_Api.Models.Models;
using static Parking_Api.Requests.ParkingSessionRequest;

namespace Parking_Api.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private const string ActiveSessionStatus = "Активна";
        private const string CompletedSessionStatus = "Завершена";
        private const string BusySpotStatus = "Занято";
        private const string FreeSpotStatus = "Свободно";

        private readonly ContextDb _contextDb;
        private readonly IHubContext<ParkingHub> _hubContext;

        public ParkingSessionService(ContextDb contextDb, IHubContext<ParkingHub> hubContext)
        {
            _contextDb = contextDb;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> GetAllSessions()
        {
            await ReleaseExpiredSessions();

            var list = await _contextDb.ParkingSessions.Include(x => x.user).Include(x => x.car).Include(x => x.parkingComplex).Include(x => x.parkingSpot).Include(x => x.subscription).OrderByDescending(x => x.entry_time).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetSessionsByUser(int user_id)
        {
            await ReleaseExpiredSessions();

            var list = await _contextDb.ParkingSessions
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Include(x => x.subscription)
                .Where(x => x.user_id == user_id)
                .OrderByDescending(x => x.entry_time)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetActiveSessions()
        {
            await ReleaseExpiredSessions();

            var now = DateTime.UtcNow;
            var list = await _contextDb.ParkingSessions
                .Include(x => x.user)
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Where(x => x.status == ActiveSessionStatus && (x.exit_time == null || x.exit_time > now))
                .OrderByDescending(x => x.entry_time)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateSession(CreateSessionRequest sessionModel)
        {
            try
            {
                if (sessionModel.duration_minutes <= 0)
                    return new OkObjectResult(new { status = false, message = "Укажите длительность парковки" });

                var now = DateTime.UtcNow;
                await ReleaseExpiredSessions(now);

                var user = await _contextDb.Users
                    .FirstOrDefaultAsync(x => x.id_user == sessionModel.user_id);

                if (user == null)
                    return new OkObjectResult(new { status = false, message = "Пользователь не найден" });

                var car = await _contextDb.Cars
                    .FirstOrDefaultAsync(x => x.id_car == sessionModel.car_id && x.user_id == sessionModel.user_id);

                if (car == null)
                    return new OkObjectResult(new { status = false, message = "Машина не найдена" });

                var activeCarSession = await _contextDb.ParkingSessions
                    .Include(x => x.parkingComplex)
                    .Include(x => x.parkingSpot)
                    .FirstOrDefaultAsync(x => x.car_id == sessionModel.car_id
                        && x.parking_complex_id == sessionModel.parking_complex_id
                        && x.status == ActiveSessionStatus
                        && (x.exit_time == null || x.exit_time > now));

                if (activeCarSession != null)
                {
                    var paidUntil = activeCarSession.exit_time?.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
                        ?? "завершения текущей парковки";
                    return new OkObjectResult(new
                    {
                        status = false,
                        message = $"Для этой машины уже оплачена парковка в этом комплексе до {paidUntil}"
                    });
                }

                var complex = await _contextDb.ParkingComplexes
                    .FirstOrDefaultAsync(x => x.id_complex == sessionModel.parking_complex_id);

                if (complex == null)
                    return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

                var spot = await _contextDb.ParkingSpots
                    .FirstOrDefaultAsync(x => x.id_spot == sessionModel.parking_spot_id
                        && x.parking_complex_id == sessionModel.parking_complex_id);

                if (spot == null)
                    return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

                if (spot.status == BusySpotStatus)
                    return new OkObjectResult(new { status = false, message = "Парковочное место уже занято" });

                if (sessionModel.subscription_id != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var subscription = await _contextDb.Subscriptions
                        .Include(x => x.subscriptionPlan)
                        .FirstOrDefaultAsync(x => x.id_subscription == sessionModel.subscription_id
                            && x.user_id == sessionModel.user_id);

                    if (subscription == null)
                        return new OkObjectResult(new { status = false, message = "Абонемент не найден" });

                    if (subscription.end_date < today)
                        return new OkObjectResult(new { status = false, message = "Срок абонемента закончился" });

                    if (subscription.status != "Активно")
                        return new OkObjectResult(new { status = false, message = "Абонемент не активен" });

                    if (subscription.subscriptionPlan?.parking_complex_id != sessionModel.parking_complex_id)
                        return new OkObjectResult(new { status = false, message = "Абонемент оформлен для другого комплекса" });
                }

                var parkingSession = new ParkingSessionModel
                {
                    user_id = sessionModel.user_id,
                    car_id = sessionModel.car_id,
                    parking_complex_id = sessionModel.parking_complex_id,
                    parking_spot_id = sessionModel.parking_spot_id,
                    subscription_id = sessionModel.subscription_id,
                    entry_time = now,
                    exit_time = now.AddMinutes(sessionModel.duration_minutes),
                    status = ActiveSessionStatus
                };

                spot.status = BusySpotStatus;

                await _contextDb.ParkingSessions.AddAsync(parkingSession);
                _contextDb.ParkingSpots.Update(spot);

                await _contextDb.SaveChangesAsync();

                await NotifySpotChanged(spot, "updated");

                return new OkObjectResult(new
                {
                    status = true,
                    message = "Парковочная сессия начата",
                    session = parkingSession
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    status = false,
                    message = $"Ошибка оформления парковки: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> CreateGuestSession(GuestSessionRequest sessionModel)
        {
            if (string.IsNullOrWhiteSpace(sessionModel.license_plate))
                return new OkObjectResult(new { status = false, message = "Введите гос. номер машины" });

            var plate = sessionModel.license_plate.Trim().ToUpperInvariant();
            var car = await _contextDb.Cars.FirstOrDefaultAsync(x => x.license_plate == plate);

            if (car == null)
            {
                var guestUser = new UserModel
                {
                    name = "Гость",
                    surname = plate,
                    email = $"guest_{Guid.NewGuid():N}@parking.local",
                    phone = plate,
                    password = Guid.NewGuid().ToString("N"),
                    role_id = 0
                };

                await _contextDb.Users.AddAsync(guestUser);
                await _contextDb.SaveChangesAsync();

                car = new CarModel
                {
                    license_plate = plate,
                    brand = "Гость",
                    model = "Разовый въезд",
                    user_id = guestUser.id_user,
                    user = null
                };

                await _contextDb.Cars.AddAsync(car);
                await _contextDb.SaveChangesAsync();
            }

            return await CreateSession(new CreateSessionRequest
            {
                user_id = car.user_id,
                car_id = car.id_car,
                parking_complex_id = sessionModel.parking_complex_id,
                parking_spot_id = sessionModel.parking_spot_id,
                duration_minutes = sessionModel.duration_minutes
            });
        }

        public async Task<IActionResult> CloseSession(int session_id)
        {
            var session = await _contextDb.ParkingSessions
                .FirstOrDefaultAsync(x => x.id_session == session_id);

            if (session == null)
                return new OkObjectResult(new { status = false, message = "Парковочная сессия не найдена" });

            if (session.status == CompletedSessionStatus)
                return new OkObjectResult(new { status = false, message = "Парковочная сессия уже завершена" });

            var spot = await _contextDb.ParkingSpots
                .FirstOrDefaultAsync(x => x.id_spot == session.parking_spot_id);

            session.exit_time = DateTime.UtcNow;
            session.status = CompletedSessionStatus;

            if (spot != null)
            {
                spot.status = FreeSpotStatus;
                _contextDb.ParkingSpots.Update(spot);
            }

            _contextDb.ParkingSessions.Update(session);
            await _contextDb.SaveChangesAsync();

            if (spot != null)
                await NotifySpotChanged(spot, "updated");

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочная сессия завершена"
            });
        }

        private async Task ReleaseExpiredSessions(DateTime? currentTime = null)
        {
            var now = currentTime ?? DateTime.UtcNow;
            var expiredSessions = await _contextDb.ParkingSessions
                .Where(x => x.status == ActiveSessionStatus && x.exit_time != null && x.exit_time <= now)
                .ToListAsync();

            if (expiredSessions.Count == 0)
                return;

            var spotIds = expiredSessions.Select(x => x.parking_spot_id).Distinct().ToList();
            var spots = await _contextDb.ParkingSpots
                .Where(x => spotIds.Contains(x.id_spot))
                .ToListAsync();

            foreach (var session in expiredSessions)
                session.status = CompletedSessionStatus;

            foreach (var spot in spots)
                spot.status = FreeSpotStatus;

            _contextDb.ParkingSessions.UpdateRange(expiredSessions);
            _contextDb.ParkingSpots.UpdateRange(spots);
            await _contextDb.SaveChangesAsync();

            foreach (var spot in spots)
                await NotifySpotChanged(spot, "updated");
        }

        private async Task NotifySpotChanged(ParkingSpotModel spot, string changeType)
        {
            spot.parkingComplex = null;

            await _hubContext.Clients
                .Group(ParkingHub.GetComplexGroupName(spot.parking_complex_id))
                .SendAsync("ParkingSpotChanged", spot, changeType);
        }
    }
}
