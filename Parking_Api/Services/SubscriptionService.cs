using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Data;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private const string ActiveSubscriptionStatus = "Активно";
        private const string FinishedSubscriptionStatus = "Окончено";

        private readonly ContextDb _ContextDb;

        public SubscriptionService(ContextDb ContextDb)
        {
            _ContextDb = ContextDb;
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
            var today = DateOnly.FromDateTime(DateTime.Now);

            var user = await _ContextDb.Users.FirstOrDefaultAsync(x => x.id_user == subscriptionModel.user_id);

            if (user == null)
                return new OkObjectResult(new { status = false, message = "Пользователь не найден" });

            var plan = await _ContextDb.SubscriptionPlans.FirstOrDefaultAsync(x => x.id_plan == subscriptionModel.subscription_plan_id);

            if (plan == null)
                return new OkObjectResult(new { status = false, message = "Тариф абонемента не найден" });

            var existingSubscriptions = await _ContextDb.Subscriptions
                .Include(x => x.subscriptionPlan)
                .Where(x => x.user_id == subscriptionModel.user_id
                    && x.subscriptionPlan != null
                    && x.subscriptionPlan.parking_complex_id == plan.parking_complex_id
                    && x.end_date >= today
                    && x.status == ActiveSubscriptionStatus)
                .OrderByDescending(x => x.end_date)
                .ToListAsync();

            var existingSubscription = existingSubscriptions.FirstOrDefault();

            if (existingSubscription != null)
            {
                existingSubscription.end_date = existingSubscription.end_date.AddDays(plan.duration_days);
                existingSubscription.status = ActiveSubscriptionStatus;

                foreach (var duplicate in existingSubscriptions.Where(x => x.id_subscription != existingSubscription.id_subscription))
                    duplicate.status = FinishedSubscriptionStatus;

                _ContextDb.Subscriptions.Update(existingSubscription);
                _ContextDb.Subscriptions.UpdateRange(existingSubscriptions.Where(x => x.id_subscription != existingSubscription.id_subscription));
                await _ContextDb.SaveChangesAsync();

                existingSubscription.user = null;
                existingSubscription.subscriptionPlan = null;

                return new OkObjectResult(new
                {
                    status = true,
                    message = $"Абонемент продлен до {existingSubscription.end_date:dd.MM.yyyy}",
                    subscription = existingSubscription
                });
            }

            subscriptionModel.start_date = today;
            subscriptionModel.end_date = subscriptionModel.start_date.AddDays(plan.duration_days);
            subscriptionModel.status = ActiveSubscriptionStatus;

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

    }
}
