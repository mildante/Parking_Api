using Microsoft.AspNetCore.Mvc;
using Parking_Api.Services;
using static Parking_Api.Models.Models;

namespace Parking_Api.Controllers
{
    public class ParkingController : Controller
    {
        private readonly IParkingService _parkingService;

        public ParkingController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        [HttpGet("getAllComplexes")]
        public async Task<IActionResult> GetAllComplexes()
        {
            return await _parkingService.GetAllComplexes();
        }

        [HttpPost("createComplex")]
        public async Task<IActionResult> CreateComplex([FromBody] ParkingComplexModel complexModel)
        {
            return await _parkingService.CreateComplex(complexModel);
        }

        [HttpPut("updateComplex")]
        public async Task<IActionResult> UpdateComplex([FromBody] ParkingComplexModel complexModel)
        {
            return await _parkingService.UpdateComplex(complexModel);
        }

        [HttpDelete("deleteComplex/{complex_id}")]
        public async Task<IActionResult> DeleteComplex(int complex_id)
        {
            return await _parkingService.DeleteComplex(complex_id);
        }

        [HttpGet("getAllSpots")]
        public async Task<IActionResult> GetAllSpots()
        {
            return await _parkingService.GetAllSpots();
        }

        [HttpGet("getSpotsByComplex/{complex_id}")]
        public async Task<IActionResult> GetSpotsByComplex(int complex_id)
        {
            return await _parkingService.GetSpotsByComplex(complex_id);
        }

        [HttpPost("createSpot")]
        public async Task<IActionResult> CreateSpot([FromBody] ParkingSpotModel spotModel)
        {
            return await _parkingService.CreateSpot(spotModel);
        }

        [HttpPut("updateSpotStatus/{spot_id}")]
        public async Task<IActionResult> UpdateSpotStatus(int spot_id, [FromBody] string status)
        {
            return await _parkingService.UpdateSpotStatus(spot_id, status);
        }

        [HttpDelete("deleteSpot/{spot_id}")]
        public async Task<IActionResult> DeleteSpot(int spot_id)
        {
            return await _parkingService.DeleteSpot(spot_id);
        }
    }
}