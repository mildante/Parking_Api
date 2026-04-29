using Microsoft.AspNetCore.Mvc;
using Parking_Api.Services;
using static Parking_Api.Models.Models;

namespace Parking_Api.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet("getAllCars")]
        public async Task<IActionResult> GetAllCars()
        {
            return await _carService.GetAllCars();
        }

        [HttpGet("getCarsByUser/{user_id}")]
        public async Task<IActionResult> GetCarsByUser(int user_id)
        {
            return await _carService.GetCarsByUser(user_id);
        }

        [HttpPost("createCar")]
        public async Task<IActionResult> CreateCar([FromBody] CarModel carModel)
        {
            return await _carService.CreateCar(carModel);
        }

        [HttpPut("updateCar")]
        public async Task<IActionResult> UpdateCar([FromBody] CarModel carModel)
        {
            return await _carService.UpdateCar(carModel);
        }

        [HttpDelete("deleteCar/{car_id}")]
        public async Task<IActionResult> DeleteCar(int car_id)
        {
            return await _carService.DeleteCar(car_id);
        }
    }
}