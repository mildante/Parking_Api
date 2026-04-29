using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Parking_Api.Requests.UserRequest;

namespace Parking_Api.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> AuthByToken(ClaimsPrincipal claims);
        Task<IActionResult> RegistrationUser(UserModelRequest userModel);
        Task<IActionResult> AuthUser(AuthUserRequest authModel);
        Task<IActionResult> UpdateUser(UserModelRequest userModel);
        Task<IActionResult> DeleteUser(int user_id);
        Task<IActionResult> GetAllUser();
    }
}
