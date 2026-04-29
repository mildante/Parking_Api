using Microsoft.AspNetCore.Mvc;
using static Parking_Api.Models.Models;

namespace Parking_Api.Services
{
    public interface ISubscriptionService
    {
        Task<IActionResult> GetAllPlans();
        Task<IActionResult> GetPlansByComplex(int complex_id);
        Task<IActionResult> CreatePlan(SubscriptionPlanModel planModel);
        Task<IActionResult> UpdatePlan(SubscriptionPlanModel planModel);
        Task<IActionResult> DeletePlan(int plan_id);

        Task<IActionResult> GetAllSubscriptions();
        Task<IActionResult> GetSubscriptionsByUser(int user_id);
        Task<IActionResult> CreateSubscription(SubscriptionModel subscriptionModel);
        Task<IActionResult> UpdateSubscriptionStatus(int subscription_id, string status);
        Task<IActionResult> DeleteSubscription(int subscription_id);
    }
}