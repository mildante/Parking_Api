using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using Parking_Api.Hubs;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ContextDb _contextDb;
        private readonly IHubContext<ParkingHub> _hubContext;

        public ParkingService(ContextDb contextDb, IHubContext<ParkingHub> hubContext)
        {
            _contextDb = contextDb;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> GetAllComplexes()
        {
            var list = await _contextDb.ParkingComplexes
                .OrderBy(x => x.id_complex)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateComplex(ParkingComplexModel complexModel)
        {
            complexModel.name = complexModel.name?.Trim();
            complexModel.address = complexModel.address?.Trim();
            complexModel.total_spots = 0;

            if (string.IsNullOrWhiteSpace(complexModel.name) || string.IsNullOrWhiteSpace(complexModel.address))
                return new OkObjectResult(new { status = false, message = "Введите название и адрес комплекса" });

            await _contextDb.ParkingComplexes.AddAsync(complexModel);
            await _contextDb.SaveChangesAsync();

            await NotifyComplexChanged(complexModel.id_complex);

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочный комплекс добавлен",
                complex = complexModel
            });
        }

        public async Task<IActionResult> UpdateComplex(ParkingComplexModel complexModel)
        {
            var complex = await _contextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == complexModel.id_complex);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            if (string.IsNullOrWhiteSpace(complexModel.name) || string.IsNullOrWhiteSpace(complexModel.address))
                return new OkObjectResult(new { status = false, message = "Введите название и адрес комплекса" });

            complex.name = complexModel.name.Trim();
            complex.address = complexModel.address.Trim();
            complex.total_spots = await _contextDb.ParkingSpots.CountAsync(x => x.parking_complex_id == complex.id_complex);

            _contextDb.ParkingComplexes.Update(complex);
            await _contextDb.SaveChangesAsync();

            await NotifyComplexChanged(complex.id_complex);

            return new OkObjectResult(new
            {
                status = true,
                message = "Данные парковочного комплекса обновлены",
                complex
            });
        }

        public async Task<IActionResult> DeleteComplex(int complex_id)
        {
            var complex = await _contextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            var hasSpots = await _contextDb.ParkingSpots.AnyAsync(x => x.parking_complex_id == complex_id);

            if (hasSpots)
                return new OkObjectResult(new { status = false, message = "Сначала удалите парковочные места этого комплекса" });

            _contextDb.ParkingComplexes.Remove(complex);
            await _contextDb.SaveChangesAsync();

            await NotifyComplexChanged(complex_id);

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочный комплекс удален"
            });
        }

        public async Task<IActionResult> GetSpotsByComplex(int complex_id)
        {
            var list = await _contextDb.ParkingSpots
                .Where(x => x.parking_complex_id == complex_id)
                .OrderBy(x => x.number)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateSpot(ParkingSpotModel spotModel)
        {
            spotModel.number = spotModel.number?.Trim();

            if (string.IsNullOrWhiteSpace(spotModel.number))
                return new OkObjectResult(new { status = false, message = "Введите номер места" });

            var complex = await _contextDb.ParkingComplexes
                .FirstOrDefaultAsync(x => x.id_complex == spotModel.parking_complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            var isNumberNotUnique = await _contextDb.ParkingSpots
                .AnyAsync(x => x.number == spotModel.number && x.parking_complex_id == spotModel.parking_complex_id);

            if (isNumberNotUnique)
                return new OkObjectResult(new { status = false, message = "Место с таким номером уже существует" });

            var normalizedStatus = NormalizeSpotStatus(spotModel.status);

            if (normalizedStatus == null)
                return new OkObjectResult(new { status = false, message = "Некорректный статус" });

            spotModel.status = normalizedStatus;
            spotModel.parkingComplex = null;

            await _contextDb.ParkingSpots.AddAsync(spotModel);
            complex.total_spots = await _contextDb.ParkingSpots.CountAsync(x => x.parking_complex_id == spotModel.parking_complex_id) + 1;
            _contextDb.ParkingComplexes.Update(complex);

            await _contextDb.SaveChangesAsync();

            await NotifySpotChanged(spotModel, "created");
            await NotifyComplexChanged(spotModel.parking_complex_id);

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочное место добавлено",
                spot = spotModel
            });
        }

        public async Task<IActionResult> UpdateSpotStatus(int spot_id, string status)
        {
            var spot = await _contextDb.ParkingSpots.FirstOrDefaultAsync(x => x.id_spot == spot_id);

            if (spot == null)
                return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

            var normalizedStatus = NormalizeSpotStatus(status);

            if (normalizedStatus == null)
                return new OkObjectResult(new { status = false, message = "Некорректный статус" });

            spot.status = normalizedStatus;

            _contextDb.ParkingSpots.Update(spot);
            await _contextDb.SaveChangesAsync();

            await NotifySpotChanged(spot, "updated");

            return new OkObjectResult(new
            {
                status = true,
                message = "Статус обновлен",
                spot
            });
        }

        public async Task<IActionResult> DeleteSpot(int spot_id)
        {
            var spot = await _contextDb.ParkingSpots.FirstOrDefaultAsync(x => x.id_spot == spot_id);

            if (spot == null)
                return new OkObjectResult(new { status = false, message = "Парковочное место не найдено" });

            var now = DateTime.UtcNow;
            var hasActiveSession = await _contextDb.ParkingSessions
                .AnyAsync(x => x.parking_spot_id == spot_id
                    && x.status == "Активна"
                    && (x.exit_time == null || x.exit_time > now));

            if (hasActiveSession)
                return new OkObjectResult(new { status = false, message = "Нельзя удалить место с активной парковочной сессией" });

            var complexId = spot.parking_complex_id;

            _contextDb.ParkingSpots.Remove(spot);

            var complex = await _contextDb.ParkingComplexes.FirstOrDefaultAsync(x => x.id_complex == complexId);

            if (complex != null)
            {
                complex.total_spots = Math.Max(0, await _contextDb.ParkingSpots.CountAsync(x => x.parking_complex_id == complexId) - 1);
                _contextDb.ParkingComplexes.Update(complex);
            }

            await _contextDb.SaveChangesAsync();

            await NotifySpotChanged(spot, "deleted");
            await NotifyComplexChanged(complexId);

            return new OkObjectResult(new
            {
                status = true,
                message = "Парковочное место удалено"
            });
        }

        private static string? NormalizeSpotStatus(string? status)
        {
            return status?.Trim().ToLowerInvariant() switch
            {
                "занято" => "Занято",
                "свободно" => "Свободно",
                _ => null
            };
        }

        private async Task NotifyComplexChanged(int complexId)
        {
            await _hubContext.Clients.All.SendAsync("ParkingComplexChanged", complexId);
        }

        private async Task NotifySpotChanged(ParkingSpotModel spot, string changeType)
        {
            spot.parkingComplex = null;

            await _hubContext.Clients
                .Group(ParkingHub.GetComplexGroupName(spot.parking_complex_id))
                .SendAsync("ParkingSpotChanged", spot, changeType);
        }
    }
}
