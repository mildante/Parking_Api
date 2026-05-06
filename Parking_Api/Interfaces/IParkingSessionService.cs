using Microsoft.AspNetCore.Mvc;
using static Parking_Api.Requests.ParkingSessionRequest;

namespace Parking_Api.Services
{
    public interface IParkingSessionService
    {
        Task<IActionResult> GetAllSessions();
        Task<IActionResult> GetSessionsByUser(int user_id);
        Task<IActionResult> GetActiveSessions();

        Task<IActionResult> CreateSession(CreateSessionRequest sessionModel);
        Task<IActionResult> CreateGuestSession(GuestSessionRequest sessionModel);
        Task<IActionResult> CloseSession(int session_id);
    }
}
