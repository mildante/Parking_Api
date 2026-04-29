using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parking_Api.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingComplexes",
                columns: table => new
                {
                    id_complex = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    total_spots = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingComplexes", x => x.id_complex);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    surname = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id_user);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    id_spot = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    parking_complex_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.id_spot);
                    table.ForeignKey(
                        name: "FK_ParkingSpots_ParkingComplexes_parking_complex_id",
                        column: x => x.parking_complex_id,
                        principalTable: "ParkingComplexes",
                        principalColumn: "id_complex",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    id_plan = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    parking_complex_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.id_plan);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_ParkingComplexes_parking_complex_id",
                        column: x => x.parking_complex_id,
                        principalTable: "ParkingComplexes",
                        principalColumn: "id_complex",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    id_car = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    license_plate = table.Column<string>(type: "text", nullable: false),
                    brand = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.id_car);
                    table.ForeignKey(
                        name: "FK_Cars_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    id_subscription = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    subscription_plan_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.id_subscription);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "id_plan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    id_session = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    car_id = table.Column<int>(type: "integer", nullable: false),
                    parking_complex_id = table.Column<int>(type: "integer", nullable: false),
                    parking_spot_id = table.Column<int>(type: "integer", nullable: false),
                    subscription_id = table.Column<int>(type: "integer", nullable: true),
                    entry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    exit_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.id_session);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Cars_car_id",
                        column: x => x.car_id,
                        principalTable: "Cars",
                        principalColumn: "id_car",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_ParkingComplexes_parking_complex_id",
                        column: x => x.parking_complex_id,
                        principalTable: "ParkingComplexes",
                        principalColumn: "id_complex",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_ParkingSpots_parking_spot_id",
                        column: x => x.parking_spot_id,
                        principalTable: "ParkingSpots",
                        principalColumn: "id_spot",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "Subscriptions",
                        principalColumn: "id_subscription");
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_user_id",
                table: "Cars",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_car_id",
                table: "ParkingSessions",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_parking_complex_id",
                table: "ParkingSessions",
                column: "parking_complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_parking_spot_id",
                table: "ParkingSessions",
                column: "parking_spot_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_subscription_id",
                table: "ParkingSessions",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_user_id",
                table: "ParkingSessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_parking_complex_id",
                table: "ParkingSpots",
                column: "parking_complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_parking_complex_id",
                table: "SubscriptionPlans",
                column: "parking_complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_subscription_plan_id",
                table: "Subscriptions",
                column: "subscription_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_user_id",
                table: "Subscriptions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ParkingComplexes");
        }
    }
}
