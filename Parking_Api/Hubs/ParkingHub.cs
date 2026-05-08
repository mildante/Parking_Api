using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Hubs
{
    public class ParkingHub : Hub
    {
        public const int ParkingWarningMinutes = 15;

        private const string ActiveSessionStatus = "Активна";
        private const string CompletedSessionStatus = "Завершена";
        private const string FreeSpotStatus = "Свободно";

        private readonly ContextDb _contextDb;

        public ParkingHub(ContextDb contextDb)
        {
            _contextDb = contextDb;
        }

        public async Task JoinParkingComplex(int complexId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetComplexGroupName(complexId));
            await CheckExpiredSessions(complexId);
        }

        public async Task LeaveParkingComplex(int complexId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetComplexGroupName(complexId));
        }

        public async Task CheckExpiredSessions(int complexId)
        {
            var now = DateTime.UtcNow;

            var expiredSessions = await _contextDb.ParkingSessions
                .Where(x => x.parking_complex_id == complexId
                    && x.status == ActiveSessionStatus
                    && x.exit_time != null
                    && x.exit_time <= now)
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

            await _contextDb.SaveChangesAsync();

            foreach (var spot in spots)
            {
                var payload = new ParkingSpotModel
                {
                    id_spot = spot.id_spot,
                    number = spot.number,
                    status = spot.status,
                    parking_complex_id = spot.parking_complex_id,
                    parkingComplex = null
                };

                await Clients.Group(GetComplexGroupName(complexId))
                    .SendAsync("ParkingSpotChanged", payload, "updated");
            }
        }

        public async Task JoinUserNotifications(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
            await SendSubscriptionWarnings(userId);
            await SendParkingSessionWarnings(userId);
        }

        public async Task CheckSubscriptionWarnings(int userId)
        {
            await SendSubscriptionWarnings(userId);
        }

        public async Task CheckParkingSessionWarnings(int userId)
        {
            await SendParkingSessionWarnings(userId);
        }

        public static string GetComplexGroupName(int complexId)
        {
            return $"parking_complex_{complexId}";
        }

        public static string GetUserGroupName(int userId)
        {
            return $"parking_user_{userId}";
        }

        private async Task SendSubscriptionWarnings(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var warningDate = today.AddDays(1);

            var subscriptions = await _contextDb.Subscriptions
                .Include(x => x.subscriptionPlan)
                .ThenInclude(x => x.parkingComplex)
                .Where(x => x.user_id == userId
                    && x.status == "Активно"
                    && x.end_date >= today
                    && x.end_date <= warningDate)
                .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                var place = subscription.subscriptionPlan?.parkingComplex?.name ?? "парковку";
                var dayText = subscription.end_date == today ? "сегодня" : "завтра";

                await Clients.Caller.SendAsync(
                    "ParkingNotification",
                    $"Абонемент на {place} заканчивается {dayText}.");
            }
        }

        private async Task SendParkingSessionWarnings(int userId)
        {
            var now = DateTime.UtcNow;
            var warningLimit = now.AddMinutes(ParkingWarningMinutes);

            var sessions = await _contextDb.ParkingSessions
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Where(x => x.user_id == userId
                    && x.status == ActiveSessionStatus
                    && x.exit_time != null
                    && x.exit_time > now
                    && x.exit_time <= warningLimit)
                .ToListAsync();

            foreach (var session in sessions)
            {
                var place = session.parkingComplex?.name ?? "парковке";
                var plate = session.car?.license_plate ?? "автомобиля";
                var spot = session.parkingSpot?.number ?? "";
                var minutesLeft = Math.Max(1, (int)Math.Ceiling((session.exit_time!.Value - now).TotalMinutes));
                var spotText = string.IsNullOrWhiteSpace(spot) ? "" : $", место {spot}";

                await Clients.Caller.SendAsync(
                    "ParkingNotification",
                    $"Оплаченная парковка для {plate} в {place}{spotText} заканчивается через {minutesLeft} мин.");
            }
        }
    }
}