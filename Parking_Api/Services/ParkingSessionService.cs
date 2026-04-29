using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly ContextDb _ContextDb;

        public ParkingSessionService(ContextDb ContextDb)
        {
            _ContextDb = ContextDb;
        }

        public async Task<IActionResult> GetAllSessions()
        {
            var list = await _ContextDb.ParkingSessions
                .Include(x => x.user)
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Include(x => x.subscription)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetSessionsByUser(int user_id)
        {
            var list = await _ContextDb.ParkingSessions
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Include(x => x.subscription)
                .Where(x => x.user_id == user_id)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetActiveSessions()
        {
            var list = await _ContextDb.ParkingSessions
                .Include(x => x.user)
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Where(x => x.status == "Занято")
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateSession(ParkingSessionModel sessionModel)
        {
            var user = await _ContextDb.Users
                .FirstOrDefaultAsync(x => x.id_user == sessionModel.user_id);

            if (user == null)
                return new OkObjectResult(new { status = false, message = "Пользователь не найден" });

            var car = await _ContextDb.Cars
                .FirstOrDefaultAsync(x => x.id_car == sessionModel.car_id);

            if (car == null)
                return new OkObjectResult(new { status = false, message = "Машина не найдена" });

            var complex = await _ContextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == sessionModel.parking_complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            var spot = await _ContextDb.ParkingSpots
                .FirstOrDefaultAsync(x => x.id_spot == sessionModel.parking_spot_id);

            if (spot == null)
                return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

            if (spot.status == "Занято" || spot.status == "occupied")
                return new OkObjectResult(new { status = false, message = "Парковочное место уже занято" });

            if (sessionModel.subscription_id != null)
            {
                var subscription = await _ContextDb.Subscriptions
                    .FirstOrDefaultAsync(x => x.id_subscription == sessionModel.subscription_id);

                if (subscription == null)
                    return new OkObjectResult(new { status = false, message = "Абонемент не найден" });
            }

            sessionModel.entry_time = DateTime.Now;
            sessionModel.exit_time = null;
            sessionModel.status = "Занято";

            sessionModel.user = null;
            sessionModel.car = null;
            sessionModel.parkingComplex = null;
            sessionModel.parkingSpot = null;
            sessionModel.subscription = null;

            spot.status = "Занято";

            await _ContextDb.ParkingSessions.AddAsync(sessionModel);
            _ContextDb.ParkingSpots.Update(spot);

            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочная сессия начата",
                session = sessionModel
            });
        }

        public async Task<IActionResult> CloseSession(int session_id)
        {
            var session = await _ContextDb.ParkingSessions
                .FirstOrDefaultAsync(x => x.id_session == session_id);

            if (session == null)
                return new OkObjectResult(new { status = false, message = "Парковочная сессия не найдена" });

            var spot = await _ContextDb.ParkingSpots
                .FirstOrDefaultAsync(x => x.id_spot == session.parking_spot_id);

            session.exit_time = DateTime.Now;
            session.status = "Свободно";

            if (spot != null)
            {
                spot.status = "Свободно";
                _ContextDb.ParkingSpots.Update(spot);
            }

            _ContextDb.ParkingSessions.Update(session);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочная сессия завершена"
            });
        }

        public async Task<IActionResult> DeleteSession(int session_id)
        {
            var session = await _ContextDb.ParkingSessions
                .FirstOrDefaultAsync(x => x.id_session == session_id);

            if (session == null)
                return new OkObjectResult(new { status = false, message = "Парковочная сессия не найдена" });

            _ContextDb.ParkingSessions.Remove(session);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочная сессия удалена"
            });
        }
    }
}