using Microsoft.AspNetCore.Mvc;
using Parking_Api.Services;

namespace Parking_Api.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("getAdminReport")]
        public async Task<IActionResult> GetAdminReport([FromQuery] int days = 30)
        {
            return await _reportService.GetAdminReport(days);
        }
    }
}
