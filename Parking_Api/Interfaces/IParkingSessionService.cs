using Microsoft.AspNetCore.Mvc;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public interface IParkingSessionService
    {
        Task<IActionResult> GetAllSessions();
        Task<IActionResult> GetSessionsByUser(int user_id);
        Task<IActionResult> GetActiveSessions();

        Task<IActionResult> CreateSession(ParkingSessionModel sessionModel);
        Task<IActionResult> CloseSession(int session_id);
        Task<IActionResult> DeleteSession(int session_id);
    }
}