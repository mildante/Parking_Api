using static Parking_Api.Models.Models;

namespace Parking_Api.Requests
{
    public class UserRequest
    {
        public class UserModelRequest
        {
            public int id_user { get; set; }
            public string name { get; set; }
            public string? surname { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string password { get; set; }
            public int role_id { get; set; }
        }
        public class AuthUserRequest
        {
            public string email { get; set; }
            public string password { get; set; }
        }
    }
}
