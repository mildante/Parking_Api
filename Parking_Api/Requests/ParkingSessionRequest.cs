namespace Parking_Api.Requests
{
    public class ParkingSessionRequest
    {
        public class CreateSessionRequest
        {
            public int user_id { get; set; }
            public int car_id { get; set; }
            public int parking_complex_id { get; set; }
            public int parking_spot_id { get; set; }
            public int? subscription_id { get; set; }
            public int duration_minutes { get; set; }
        }

        public class GuestSessionRequest
        {
            public string license_plate { get; set; }
            public int parking_complex_id { get; set; }
            public int parking_spot_id { get; set; }
            public int duration_minutes { get; set; }
        }
    }
}
