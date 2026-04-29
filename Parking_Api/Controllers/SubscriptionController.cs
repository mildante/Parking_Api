using Microsoft.AspNetCore.Mvc;
using Parking_Api.Services;
using static Parking_Api.Models.Models;

namespace Parking_Api.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("getAllPlans")]
        public async Task<IActionResult> GetAllPlans()
        {
            return await _subscriptionService.GetAllPlans();
        }

        [HttpGet("getPlansByComplex/{complex_id}")]
        public async Task<IActionResult> GetPlansByComplex(int complex_id)
        {
            return await _subscriptionService.GetPlansByComplex(complex_id);
        }

        [HttpPost("createPlan")]
        public async Task<IActionResult> CreatePlan([FromBody] SubscriptionPlanModel planModel)
        {
            return await _subscriptionService.CreatePlan(planModel);
        }

        [HttpPut("updatePlan")]
        public async Task<IActionResult> UpdatePlan([FromBody] SubscriptionPlanModel planModel)
        {
            return await _subscriptionService.UpdatePlan(planModel);
        }

        [HttpDelete("deletePlan/{plan_id}")]
        public async Task<IActionResult> DeletePlan(int plan_id)
        {
            return await _subscriptionService.DeletePlan(plan_id);
        }

        [HttpGet("getAllSubscriptions")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            return await _subscriptionService.GetAllSubscriptions();
        }

        [HttpGet("getSubscriptionsByUser/{user_id}")]
        public async Task<IActionResult> GetSubscriptionsByUser(int user_id)
        {
            return await _subscriptionService.GetSubscriptionsByUser(user_id);
        }

        [HttpPost("createSubscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionModel subscriptionModel)
        {
            return await _subscriptionService.CreateSubscription(subscriptionModel);
        }

        [HttpPut("updateSubscriptionStatus/{subscription_id}")]
        public async Task<IActionResult> UpdateSubscriptionStatus(int subscription_id, [FromBody] string status)
        {
            return await _subscriptionService.UpdateSubscriptionStatus(subscription_id, status);
        }

        [HttpDelete("deleteSubscription/{subscription_id}")]
        public async Task<IActionResult> DeleteSubscription(int subscription_id)
        {
            return await _subscriptionService.DeleteSubscription(subscription_id);
        }
    }
}