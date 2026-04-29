
using Microsoft.AspNetCore.Mvc;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public interface ICarService
    {
        Task<IActionResult> GetAllCars();
        Task<IActionResult> GetCarsByUser(int user_id);
        Task<IActionResult> CreateCar(CarModel carModel);
        Task<IActionResult> UpdateCar(CarModel carModel);
        Task<IActionResult> DeleteCar(int car_id);
    }
}