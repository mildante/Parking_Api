using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class CarService : ICarService
    {
        private readonly ContextDb _ContextDb;

        public CarService(ContextDb ContextDb)
        {
            _ContextDb = ContextDb;
        }

        public async Task<IActionResult> GetAllCars()
        {
            var list = await _ContextDb.Cars.Include(x => x.user).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetCarsByUser(int user_id)
        {
            var list = await _ContextDb.Cars.Where(x => x.user_id == user_id).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateCar(CarModel carModel)
        {
            var user = await _ContextDb.Users.FirstOrDefaultAsync(x => x.id_user == carModel.user_id);

            if (user == null)
                return new OkObjectResult(new { status = false, message = "Пользователь не найден" });

            var isPlateNotUnique = await _ContextDb.Cars.AnyAsync(x => x.license_plate == carModel.license_plate);

            if (isPlateNotUnique)
                return new OkObjectResult(new { status = false, message = "Машина с таким номером уже добавлена" });

            await _ContextDb.Cars.AddAsync(carModel);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Машина успешно добавлена",
                car = carModel
            });
        }

        public async Task<IActionResult> UpdateCar(CarModel carModel)
        {
            var car = await _ContextDb.Cars.FirstOrDefaultAsync(x => x.id_car == carModel.id_car);

            if (car == null)
                return new OkObjectResult(new 
                { 
                    status = false, 
                    message = "Машина не найдена" 
                });

            var isPlateNotUnique = await _ContextDb.Cars.AnyAsync(x => x.license_plate == carModel.license_plate && x.id_car != carModel.id_car);

            if (isPlateNotUnique)
                return new OkObjectResult(new 
                {
                    status = false, 
                    message = "Машина с таким номером уже существует" 
                });

            car.license_plate = carModel.license_plate;
            car.brand = carModel.brand;
            car.model = carModel.model;
            car.color = carModel.color;
            car.user_id = carModel.user_id;

            _ContextDb.Cars.Update(car);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Данные машины обновлены"
            });
        }

        public async Task<IActionResult> DeleteCar(int car_id)
        {
            var car = await _ContextDb.Cars
                .FirstOrDefaultAsync(x => x.id_car == car_id);

            if (car == null)
                return new OkObjectResult(new 
                { 
                    status = false, 
                    message = "Машина не найдена" 
                });

            _ContextDb.Cars.Remove(car);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Машина удалена"
            });
        }
    }
}