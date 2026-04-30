using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking_Api.Interfaces;
using Parking_Api.Services;
using static Parking_Api.Requests.UserRequest;

namespace Parking_Api.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("authByToken")]
        public async Task<IActionResult> AuthByToken()
        {
            return await _userService.AuthByToken(User);
        }

        [HttpPost("registrationUser")]
        public async Task<IActionResult> RegistrationUser([FromBody] UserModelRequest userModel)
        {
            return await _userService.RegistrationUser(userModel);
        }

        [HttpPost("authUser")]
        public async Task<IActionResult> AuthUser([FromBody] AuthUserRequest authModel)
        {
            return await _userService.AuthUser(authModel);
        }

        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserModelRequest userModel)
        {
            return await _userService.UpdateUser(userModel);
        }

        [HttpDelete("deleteUser/{user_id}")]
        public async Task<IActionResult> DeleteUser(int user_id)
        {
            return await _userService.DeleteUser(user_id);
        }

        [HttpGet("getAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            return await _userService.GetAllUser();
        }
    }
}