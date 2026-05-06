using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Parking_Api.Models
{
    public class Models
    {
        public class UserModel
        {
            [Key]
            public int id_user { get; set; }

            public string name { get; set; }
            public string? surname { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string password { get; set; }
            public int role_id { get; set; }

            [JsonIgnore]
            public ICollection<CarModel> cars { get; set; } = new List<CarModel>();
            [JsonIgnore]
            public ICollection<SubscriptionModel> subscriptions { get; set; } = new List<SubscriptionModel>();
            [JsonIgnore]
            public ICollection<ParkingSessionModel> parkingSessions { get; set; } = new List<ParkingSessionModel>();
        }

        public class CarModel
        {
            [Key]
            public int id_car { get; set; }

            public string license_plate { get; set; }
            public string? brand { get; set; }
            public string? model { get; set; }
            public string? color { get; set; }

            [ForeignKey("user")]
            public int user_id { get; set; }

            public UserModel? user { get; set; }

            [JsonIgnore]
            public ICollection<ParkingSessionModel> parkingSessions { get; set; } = new List<ParkingSessionModel>();
        }

        public class ParkingComplexModel
        {
            [Key]
            public int id_complex { get; set; }

            public string name { get; set; }
            public string address { get; set; }
            public int total_spots { get; set; }

            [JsonIgnore]
            public ICollection<ParkingSpotModel> spots { get; set; } = new List<ParkingSpotModel>();
            [JsonIgnore]
            public ICollection<SubscriptionPlanModel> subscriptionPlans { get; set; } = new List<SubscriptionPlanModel>();
            [JsonIgnore]
            public ICollection<ParkingSessionModel> parkingSessions { get; set; } = new List<ParkingSessionModel>();
        }

        public class ParkingSpotModel
        {
            [Key]
            public int id_spot { get; set; }

            public string number { get; set; }
            public string status { get; set; } = "Свободно";

            [ForeignKey("parkingComplex")]
            public int parking_complex_id { get; set; }

            public ParkingComplexModel? parkingComplex { get; set; }

            [JsonIgnore]
            public ICollection<ParkingSessionModel> parkingSessions { get; set; } = new List<ParkingSessionModel>();
        }

        public class SubscriptionPlanModel
        {
            [Key]
            public int id_plan { get; set; }

            public string name { get; set; }
            public int duration_days { get; set; }
            public decimal price { get; set; }

            [ForeignKey("parkingComplex")]
            public int parking_complex_id { get; set; }

            public ParkingComplexModel? parkingComplex { get; set; }

            [JsonIgnore]
            public ICollection<SubscriptionModel> subscriptions { get; set; } = new List<SubscriptionModel>();
        }

        public class SubscriptionModel
        {
            [Key]
            public int id_subscription { get; set; }

            [ForeignKey("user")]
            public int user_id { get; set; }

            public UserModel? user { get; set; }

            [ForeignKey("subscriptionPlan")]
            public int subscription_plan_id { get; set; }

            public SubscriptionPlanModel? subscriptionPlan { get; set; }

            public DateOnly start_date { get; set; }
            public DateOnly end_date { get; set; }

            public string status { get; set; } = "Активно";

            [JsonIgnore]
            public ICollection<ParkingSessionModel> parkingSessions { get; set; } = new List<ParkingSessionModel>();
        }

        public class ParkingSessionModel
        {
            [Key]
            public int id_session { get; set; }

            [ForeignKey("user")]
            public int user_id { get; set; }

            public UserModel? user { get; set; }

            [ForeignKey("car")]
            public int car_id { get; set; }

            public CarModel? car { get; set; }

            [ForeignKey("parkingComplex")]
            public int parking_complex_id { get; set; }

            public ParkingComplexModel? parkingComplex { get; set; }

            [ForeignKey("parkingSpot")]
            public int parking_spot_id { get; set; }

            public ParkingSpotModel? parkingSpot { get; set; }

            [ForeignKey("subscription")]
            public int? subscription_id { get; set; }

            public SubscriptionModel? subscription { get; set; }

            public DateTime entry_time { get; set; }
            public DateTime? exit_time { get; set; }

            public string status { get; set; } = "Активна";
        }
    }
}
