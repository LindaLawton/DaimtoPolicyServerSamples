using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using PolicyServer.Client;

namespace Daimto.PolicyServer.WebAppSample.SchoolPolicy
{
    public class OrderMealRequirmentHandler : AuthorizationHandler<MealRequirment>
    {
        private readonly IPolicyServerClient _client;

        public OrderMealRequirmentHandler(IPolicyServerClient client)
        {
            _client = client;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MealRequirment requirement)
        {
            var user = context.User;

            if (await _client.HasPermissionAsync(user, "placeorder"))
            {
                var allowed = false;

                // Limiting the amount of food peole can order.
                if (requirement.Amount <= 10) allowed = true;
                else
                    allowed = (await _client.IsInRoleAsync(user, "customer") ||
                               await _client.IsInRoleAsync(user, "waitress") ||
                               await _client.IsInRoleAsync(user, "cook"));
                
                if (allowed)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}