using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ContextDb _ContextDb;

        public SubscriptionService(ContextDb ContextDb)
        {
            _ContextDb = ContextDb;
        }

        public async Task<IActionResult> GetAllPlans()
        {
            var list = await _ContextDb.SubscriptionPlans.Include(x => x.parkingComplex).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetPlansByComplex(int complex_id)
        {
            var list = await _ContextDb.SubscriptionPlans.Where(x => x.parking_complex_id == complex_id).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreatePlan(SubscriptionPlanModel planModel)
        {
            var complex = await _ContextDb.ParkingComplexes.FirstOrDefaultAsync(x => x.id_complex == planModel.parking_complex_id);

            if (complex == null)
                return new OkObjectResult(new { status = false, message = "Парковочный комплекс не найден" });

            planModel.parkingComplex = null;

            await _ContextDb.SubscriptionPlans.AddAsync(planModel);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Тариф абонемента добавлен",
                plan = planModel
            });
        }

        public async Task<IActionResult> UpdatePlan(SubscriptionPlanModel planModel)
        {
            var plan = await _ContextDb.SubscriptionPlans
                .FirstOrDefaultAsync(x => x.id_plan == planModel.id_plan);

            if (plan == null)
                return new OkObjectResult(new { status = false, message = "Тариф абонемента не найден" });

            plan.name = planModel.name;
            plan.duration_days = planModel.duration_days;
            plan.price = planModel.price;
            plan.parking_complex_id = planModel.parking_complex_id;

            _ContextDb.SubscriptionPlans.Update(plan);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Тариф абонемента обновлен"
            });
        }

        public async Task<IActionResult> DeletePlan(int plan_id)
        {
            var plan = await _ContextDb.SubscriptionPlans
                .FirstOrDefaultAsync(x => x.id_plan == plan_id);

            if (plan == null)
                return new OkObjectResult(new { status = false, message = "Тариф абонемента не найден" });

            _ContextDb.SubscriptionPlans.Remove(plan);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Тариф абонемента удален"
            });
        }





        public async Task<IActionResult> GetAllSubscriptions()
        {
            var list = await _ContextDb.Subscriptions.Include(x => x.user).Include(x => x.subscriptionPlan).ThenInclude(x => x.parkingComplex).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> GetSubscriptionsByUser(int user_id)
        {
            var list = await _ContextDb.Subscriptions.Include(x => x.subscriptionPlan).ThenInclude(x => x.parkingComplex).Where(x => x.user_id == user_id).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                list
            });
        }

        public async Task<IActionResult> CreateSubscription(SubscriptionModel subscriptionModel)
        {
            var user = await _ContextDb.Users
                .FirstOrDefaultAsync(x => x.id_user == subscriptionModel.user_id);

            if (user == null)
                return new OkObjectResult(new { status = false, message = "Пользователь не найден" });

            var plan = await _ContextDb.SubscriptionPlans
                .FirstOrDefaultAsync(x => x.id_plan == subscriptionModel.subscription_plan_id);

            if (plan == null)
                return new OkObjectResult(new { status = false, message = "Тариф абонемента не найден" });

            subscriptionModel.start_date = DateOnly.FromDateTime(DateTime.Now);
            subscriptionModel.end_date = subscriptionModel.start_date.AddDays(plan.duration_days);
            subscriptionModel.status = "Активная";

            subscriptionModel.user = null;
            subscriptionModel.subscriptionPlan = null;

            await _ContextDb.Subscriptions.AddAsync(subscriptionModel);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Абонемент успешно оформлен",
                subscription = subscriptionModel
            });
        }

        public async Task<IActionResult> UpdateSubscriptionStatus(int subscription_id, string status)
        {
            var subscription = await _ContextDb.Subscriptions
                .FirstOrDefaultAsync(x => x.id_subscription == subscription_id);

            if (subscription == null)
                return new OkObjectResult(new { status = false, message = "Абонемент не найден" });

            if (status != "Активен" && status != "Окончен")
                return new OkObjectResult(new { status = false, message = "Некорректный статус абонемента" });

            subscription.status = status;

            _ContextDb.Subscriptions.Update(subscription);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Статус абонемента обновлен"
            });
        }

        public async Task<IActionResult> DeleteSubscription(int subscription_id)
        {
            var subscription = await _ContextDb.Subscriptions
                .FirstOrDefaultAsync(x => x.id_subscription == subscription_id);

            if (subscription == null)
                return new OkObjectResult(new { status = false, message = "Абонемент не найден" });

            _ContextDb.Subscriptions.Remove(subscription);
            await _ContextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Абонемент удален"
            });
        }
    }
}