using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Requests.ReportResponse;

namespace Parking_Api.Services
{
    public class ReportService : IReportService
    {
        private const decimal ParkingHourPrice = 30m;

        private readonly ContextDb _contextDb;

        public ReportService(ContextDb contextDb)
        {
            _contextDb = contextDb;
        }

        public async Task<IActionResult> GetAdminReport(int days)
        {
            days = Math.Clamp(days, 1, 365);

            var now = DateTime.UtcNow;
            var periodStart = now.Date.AddDays(-(days - 1));
            var today = DateOnly.FromDateTime(DateTime.Now);

            var complexes = await _contextDb.ParkingComplexes.ToListAsync();
            var spots = await _contextDb.ParkingSpots.ToListAsync();
            var sessions = await _contextDb.ParkingSessions
                .Include(x => x.parkingComplex)
                .Where(x => x.entry_time >= periodStart)
                .ToListAsync();
            var activeSessions = await _contextDb.ParkingSessions
                .Where(x => x.status == "Активна" && (x.exit_time == null || x.exit_time > now))
                .ToListAsync();
            var subscriptions = await _contextDb.Subscriptions
                .Include(x => x.subscriptionPlan)
                .ToListAsync();

            var completedSessions = sessions
                .Where(x => x.status == "Завершена" || (x.status == "Активна" && x.exit_time != null && x.exit_time <= now))
                .ToList();

            var totalParkingRevenue = sessions.Sum(GetParkingRevenue);
            var subscriptionRevenue = subscriptions
                .Where(x => x.start_date >= DateOnly.FromDateTime(periodStart))
                .Sum(x => x.subscriptionPlan?.price ?? 0m);

            var summary = new ReportSummaryModel
            {
                totalComplexes = complexes.Count,
                totalSpots = spots.Count,
                freeSpots = spots.Count(x => IsFreeStatus(x.status)),
                busySpots = spots.Count(x => IsBusyStatus(x.status)),
                occupancyPercent = GetPercent(spots.Count(x => IsBusyStatus(x.status)), spots.Count),
                totalSessions = sessions.Count,
                activeSessions = activeSessions.Count,
                completedSessions = completedSessions.Count,
                averageParkingMinutes = GetAverageMinutes(sessions),
                parkingRevenue = Math.Round(totalParkingRevenue, 2),
                subscriptionRevenue = Math.Round(subscriptionRevenue, 2),
                activeSubscriptions = subscriptions.Count(x => IsActiveSubscription(x.status) && x.end_date >= today),
                subscribersCount = subscriptions
                    .Where(x => IsActiveSubscription(x.status) && x.end_date >= today)
                    .Select(x => x.user_id)
                    .Distinct()
                    .Count()
            };

            var complexLoads = complexes
                .Select(complex =>
                {
                    var complexSpots = spots.Where(x => x.parking_complex_id == complex.id_complex).ToList();
                    var complexSessions = sessions.Where(x => x.parking_complex_id == complex.id_complex).ToList();
                    var busySpots = complexSpots.Count(x => IsBusyStatus(x.status));

                    return new ComplexLoadModel
                    {
                        complexId = complex.id_complex,
                        complexName = complex.name,
                        totalSpots = complexSpots.Count,
                        freeSpots = complexSpots.Count(x => IsFreeStatus(x.status)),
                        busySpots = busySpots,
                        occupancyPercent = GetPercent(busySpots, complexSpots.Count),
                        sessionsCount = complexSessions.Count,
                        parkingRevenue = Math.Round(complexSessions.Sum(GetParkingRevenue), 2)
                    };
                })
                .OrderByDescending(x => x.occupancyPercent)
                .ThenBy(x => x.complexName)
                .ToList();

            var dailyStats = Enumerable.Range(0, days)
                .Select(offset =>
                {
                    var date = DateOnly.FromDateTime(periodStart.AddDays(offset));
                    var daySessions = sessions
                        .Where(x => DateOnly.FromDateTime(x.entry_time.ToLocalTime()) == date)
                        .ToList();
                    var daySubscriptions = subscriptions
                        .Where(x => x.start_date == date)
                        .ToList();

                    return new DailyReportModel
                    {
                        date = date,
                        sessionsCount = daySessions.Count,
                        averageParkingMinutes = GetAverageMinutes(daySessions),
                        parkingRevenue = Math.Round(daySessions.Sum(GetParkingRevenue), 2),
                        subscriptionRevenue = Math.Round(daySubscriptions.Sum(x => x.subscriptionPlan?.price ?? 0m), 2)
                    };
                })
                .ToList();

            return new OkObjectResult(new
            {
                status = true,
                report = new AdminReportModel
                {
                    summary = summary,
                    complexLoads = complexLoads,
                    dailyStats = dailyStats
                }
            });
        }

        private static bool IsFreeStatus(string status)
        {
                return status.Equals("Свободно", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBusyStatus(string status)
        {
            return status.Equals("Занято", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsActiveSubscription(string status)
        {
            return status.Equals("Активно", StringComparison.OrdinalIgnoreCase);
        }

        private static decimal GetPercent(int value, int total)
        {
            return total == 0 ? 0 : Math.Round(value * 100m / total, 1);
        }

        private static decimal GetAverageMinutes(IEnumerable<Parking_Api.Models.Models.ParkingSessionModel> sessions)
        {
            var durations = sessions
                .Where(x => x.exit_time != null && x.exit_time > x.entry_time)
                .Select(x => (decimal)(x.exit_time!.Value - x.entry_time).TotalMinutes)
                .ToList();

            return durations.Count == 0 ? 0 : Math.Round(durations.Average(), 1);
        }

        private static decimal GetParkingRevenue(Parking_Api.Models.Models.ParkingSessionModel session)
        {
            if (session.subscription_id != null || session.exit_time == null || session.exit_time <= session.entry_time)
                return 0;

            var hours = (decimal)(session.exit_time.Value - session.entry_time).TotalMinutes / 60m;
            return Math.Max(0, hours * ParkingHourPrice);
        }
    }
}
