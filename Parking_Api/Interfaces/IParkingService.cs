using Microsoft.AspNetCore.Mvc;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public interface IParkingService
    {
        Task<IActionResult> GetAllComplexes();
        Task<IActionResult> CreateComplex(ParkingComplexModel complexModel);
        Task<IActionResult> UpdateComplex(ParkingComplexModel complexModel);
        Task<IActionResult> DeleteComplex(int complex_id);

        Task<IActionResult> GetAllSpots();
        Task<IActionResult> GetSpotsByComplex(int complex_id);
        Task<IActionResult> CreateSpot(ParkingSpotModel spotModel);
        Task<IActionResult> UpdateSpotStatus(int spot_id, string status);
        Task<IActionResult> DeleteSpot(int spot_id);
    }
}