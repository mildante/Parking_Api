using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ContextDb _ContextDb;

        public ParkingService(ContextDb ContextDb)
        {
            _ContextDb = ContextDb;
        }

        public async Task<IActionResult> GetAllComplexes()
        {
            var list = await _ContextDb.ParkingComplexes.ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateComplex(ParkingComplexModel complexModel)
        {
            await _ContextDb.ParkingComplexes.AddAsync(complexModel);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочный комплекс добавлен",
                complex = complexModel
            });
        }

        public async Task<IActionResult> UpdateComplex(ParkingComplexModel complexModel)
        {
            var complex = await _ContextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == complexModel.id_complex);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            complex.name = complexModel.name;
            complex.address = complexModel.address;
            complex.total_spots = complexModel.total_spots;

            _ContextDb.ParkingComplexes.Update(complex);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Данные парковочного комплекса обновлены"
            });
        }

        public async Task<IActionResult> DeleteComplex(int complex_id)
        {
            var complex = await _ContextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            _ContextDb.ParkingComplexes.Remove(complex);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочный комплекс удален"
            });
        }




        public async Task<IActionResult> GetAllSpots()
        {
            var list = await _ContextDb.ParkingSpots.Include(x => x.parkingComplex)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetSpotsByComplex(int complex_id)
        {
            var list = await _ContextDb.ParkingSpots
                .Where(x => x.parking_complex_id == complex_id)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateSpot(ParkingSpotModel spotModel)
        {
            var complex = await _ContextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == spotModel.parking_complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            var isNumberNotUnique = await _ContextDb.ParkingSpots.AnyAsync(x => x.number == spotModel.number && x.parking_complex_id == spotModel.parking_complex_id);

            if (isNumberNotUnique)
                return new OkObjectResult(new { status = false, message = "Место с таким номером уже существует" });

            spotModel.parkingComplex = null;

            await _ContextDb.ParkingSpots.AddAsync(spotModel);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочное место добавлено",
                spot = spotModel
            });
        }

        public async Task<IActionResult> UpdateSpotStatus(int spot_id, string status)
        {
            var spot = await _ContextDb.ParkingSpots.FirstOrDefaultAsync(x => x.id_spot == spot_id);

            if (spot == null)
                return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

            if (status != "занято" && status != "свободно")
                return new OkObjectResult(new { status = false, message = "Некорректный статус" });

            spot.status = status;

            _ContextDb.ParkingSpots.Update(spot);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Статус обновлен"
            });
        }

        public async Task<IActionResult> DeleteSpot(int spot_id)
        {
            var spot = await _ContextDb.ParkingSpots.FirstOrDefaultAsync(x => x.id_spot == spot_id);

            if (spot == null)
                return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

            _ContextDb.ParkingSpots.Remove(spot);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочное место удалено"
            });
        }
    }
}