namespace Parking_Api.Requests
{
    public class ReportResponse
    {
        public class AdminReportModel
        {
            public ReportSummaryModel summary { get; set; } = new();
            public List<ComplexLoadModel> complexLoads { get; set; } = new();
            public List<DailyReportModel> dailyStats { get; set; } = new();
        }

        public class ReportSummaryModel
        {
            public int totalComplexes { get; set; }
            public int totalSpots { get; set; }
            public int freeSpots { get; set; }
            public int busySpots { get; set; }
            public decimal occupancyPercent { get; set; }
            public int totalSessions { get; set; }
            public int activeSessions { get; set; }
            public int completedSessions { get; set; }
            public decimal averageParkingMinutes { get; set; }
            public decimal parkingRevenue { get; set; }
            public decimal subscriptionRevenue { get; set; }
            public int activeSubscriptions { get; set; }
            public int subscribersCount { get; set; }
        }

        public class ComplexLoadModel
        {
            public int complexId { get; set; }
            public string complexName { get; set; } = "";
            public int totalSpots { get; set; }
            public int freeSpots { get; set; }
            public int busySpots { get; set; }
            public decimal occupancyPercent { get; set; }
            public int sessionsCount { get; set; }
            public decimal parkingRevenue { get; set; }
        }

        public class DailyReportModel
        {
            public DateOnly date { get; set; }
            public int sessionsCount { get; set; }
            public decimal averageParkingMinutes { get; set; }
            public decimal parkingRevenue { get; set; }
            public decimal subscriptionRevenue { get; set; }
        }
    }
}
