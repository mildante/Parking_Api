using Microsoft.AspNetCore.Mvc;

namespace Parking_Api.Services
{
    public interface IReportService
    {
        Task<IActionResult> GetAdminReport(int days);
    }
}
