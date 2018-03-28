using Microsoft.AspNetCore.Authorization;

namespace Daimto.PolicyServer.WebAppSample.SchoolPolicy
{
    public class MealRequirment : IAuthorizationRequirement
    {
        public string Item { get; set; }
        public int Amount { get; set; }

        public bool isCookied { get; set; }

        public MealRequirment()
        {
            isCookied = false;
        }
    }
}