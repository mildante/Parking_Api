using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using Parking_Api.Interfaces;
using Parking_Api.Services;
using static Parking_Api.Models.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextDb>(
    option => option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped
);

builder.Services.Configure<YooKassaOptions>(
    builder.Configuration.GetSection("YooKassa"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IParkingSessionService, ParkingSessionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod());

app.UseAuthorization();

app.MapControllers();

app.Run();