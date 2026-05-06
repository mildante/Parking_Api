using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using Parking_Api.Hubs;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class ParkingNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly HashSet<string> _sentParkingWarnings = new();

        public ParkingNotificationService(
            IServiceScopeFactory scopeFactory,
            IHubContext<ParkingHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ReleaseExpiredSessions(stoppingToken);
                await SendParkingWarnings(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ReleaseExpiredSessions(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var contextDb = scope.ServiceProvider.GetRequiredService<ContextDb>();

            var expiredSessions = await contextDb.ParkingSessions
                .Where(x => x.status == "Активна"
                    && x.exit_time != null
                    && x.exit_time <= now)
                .ToListAsync(stoppingToken);

            if (expiredSessions.Count == 0)
                return;

            var spotIds = expiredSessions.Select(x => x.parking_spot_id).Distinct().ToList();
            var spots = await contextDb.ParkingSpots
                .Where(x => spotIds.Contains(x.id_spot))
                .ToListAsync(stoppingToken);

            foreach (var session in expiredSessions)
                session.status = "Завершена";

            foreach (var spot in spots)
                spot.status = "Свободно";

            contextDb.ParkingSessions.UpdateRange(expiredSessions);
            contextDb.ParkingSpots.UpdateRange(spots);
            await contextDb.SaveChangesAsync(stoppingToken);

            foreach (var spot in spots)
                await NotifySpotChanged(spot, "updated", stoppingToken);
        }
        private async Task NotifySpotChanged(ParkingSpotModel spot, string changeType, CancellationToken stoppingToken)
        {
            var payload = new ParkingSpotModel
            {
                id_spot = spot.id_spot,
                number = spot.number,
                status = spot.status,
                parking_complex_id = spot.parking_complex_id,
                parkingComplex = null
            };

            await _hubContext.Clients
                .Group(ParkingHub.GetComplexGroupName(payload.parking_complex_id))
                .SendAsync("ParkingSpotChanged", payload, changeType, stoppingToken);
        }
        private async Task SendParkingWarnings(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var warningLimit = now.AddMinutes(ParkingHub.ParkingWarningMinutes);

            using var scope = _scopeFactory.CreateScope();
            var contextDb = scope.ServiceProvider.GetRequiredService<ContextDb>();

            var sessions = await contextDb.ParkingSessions
                .Include(x => x.car)
                .Include(x => x.parkingComplex)
                .Include(x => x.parkingSpot)
                .Where(x => x.status == "Активна"
                    && x.exit_time != null
                    && x.exit_time > now
                    && x.exit_time <= warningLimit)
                .ToListAsync(stoppingToken);

            foreach (var session in sessions)
            {
                var warningKey = $"{session.id_session}_{session.exit_time:O}";

                if (!_sentParkingWarnings.Add(warningKey))
                    continue;

                var place = session.parkingComplex?.name ?? "парковке";
                var plate = session.car?.license_plate ?? "автомобиля";
                var spot = session.parkingSpot?.number ?? "";
                var minutesLeft = Math.Max(1, (int)Math.Ceiling((session.exit_time!.Value - now).TotalMinutes));
                var spotText = string.IsNullOrWhiteSpace(spot) ? "" : $", место {spot}";

                await _hubContext.Clients
                    .Group(ParkingHub.GetUserGroupName(session.user_id))
                    .SendAsync(
                        "ParkingNotification",
                        $"Оплаченная парковка для {plate} в {place}{spotText} заканчивается через {minutesLeft} мин.",
                        stoppingToken);
            }

            _sentParkingWarnings.RemoveWhere(key => !sessions.Any(session => key == $"{session.id_session}_{session.exit_time:O}"));
        }
    }
}
