using Microsoft.AspNetCore.SignalR;
using Parking_Api.Services;

namespace Parking_Api.Hubs
{
    public class ParkingHub : Hub
    {
        private readonly IParkingSessionService _parkingSessionService;

        public ParkingHub(IParkingSessionService parkingSessionService)
        {
            _parkingSessionService = parkingSessionService;
        }

    }
}
