using Microsoft.AspNetCore.Mvc;
using Parking_Api.Services;
using static Parking_Api.Requests.ParkingSessionRequest;

namespace Parking_Api.Controllers
{
    public class ParkingSessionController : Controller
    {
        private readonly IParkingSessionService _parkingSessionService;

        public ParkingSessionController(IParkingSessionService parkingSessionService)
        {
            _parkingSessionService = parkingSessionService;
        }

        [HttpGet("getAllSessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            return await _parkingSessionService.GetAllSessions();
        }

        [HttpGet("getSessionsByUser/{user_id}")]
        public async Task<IActionResult> GetSessionsByUser(int user_id)
        {
            return await _parkingSessionService.GetSessionsByUser(user_id);
        }

        [HttpGet("getActiveSessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            return await _parkingSessionService.GetActiveSessions();
        }

        [HttpPost("createSession")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest sessionModel)
        {
            return await _parkingSessionService.CreateSession(sessionModel);
        }

        [HttpPost("createGuestSession")]
        public async Task<IActionResult> CreateGuestSession([FromBody] GuestSessionRequest sessionModel)
        {
            return await _parkingSessionService.CreateGuestSession(sessionModel);
        }

        [HttpPut("closeSession/{session_id}")]
        public async Task<IActionResult> CloseSession(int session_id)
        {
            return await _parkingSessionService.CloseSession(session_id);
        }

    }
}
