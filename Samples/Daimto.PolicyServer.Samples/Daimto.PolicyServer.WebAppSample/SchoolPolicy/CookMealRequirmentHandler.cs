using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using PolicyServer.Client;

namespace Daimto.PolicyServer.WebAppSample.SchoolPolicy
{
    public class PrepareOrderRequirmentHandler : AuthorizationHandler<MealRequirment>
    {
        private readonly IPolicyServerClient _client;

        public PrepareOrderRequirmentHandler(IPolicyServerClient client)
        {
            _client = client;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MealRequirment requirement)
        {
            var user = context.User;

            if (await _client.HasPermissionAsync(user, "fillorder"))
            {
                var allowed = false;

                // Limiting the amount of food peole can order.
                if (requirement.Amount <= 10) allowed = true;
                // If the item is soda than the waitress or cook makes it.
                else if (await _client.IsInRoleAsync(user, "waitress") && requirement.Item.Equals("soda")) allowed = true;
                else allowed = await _client.IsInRoleAsync(user, "cook");

                if (allowed)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}