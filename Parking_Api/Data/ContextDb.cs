using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Numerics;
using Yandex.Checkout.V3;
using static Parking_Api.Models.Models;

namespace Parking_Api.Data
{
    public class ContextDb : DbContext
    {
        public ContextDb(DbContextOptions options) : base(options)
        {
        }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<CarModel> Cars { get; set; }

        public DbSet<ParkingComplexModel> ParkingComplexes { get; set; }
        public DbSet<ParkingSpotModel> ParkingSpots { get; set; }

        public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionModel> Subscriptions { get; set; }

        public DbSet<ParkingSessionModel> ParkingSessions { get; set; }
    }
}
