using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Daimto.PolicyServer.WebAppSample.Models;
using Daimto.PolicyServer.WebAppSample.SchoolPolicy;
using Microsoft.AspNetCore.Authorization;
using PolicyServer.Client;

//hierarchical policies, client/server separation, management APIs and UI, caching, auditing etc.,

namespace Daimto.PolicyServer.WebAppSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPolicyServerClient _client;
        private readonly IAuthorizationService _auth;

        public HomeController(IPolicyServerClient client, IAuthorizationService auth)
        {
            _client = client;
            _auth = auth;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        [AllowAnonymous]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> Secure()
        {
            var result = await _client.EvaluateAsync(User);
            return View(result);
        }

        #region Role based actions

        // The UsePolicyServerClaimsTransformation middleware, maps roles to clams 
        // enabling you to use the classic authorize attribute
        [Authorize(Roles = "cook")]
        public async Task<IActionResult> CookOnly()
        {
            // You can also use the IPolicyServerClient for the current authencated user to see if they 
            // have the role in question.  This is the same as doing [Authorize(Roles = "cook")]
            var isCook = await _client.IsInRoleAsync(User, "cook");
            if (!isCook) return Forbid();

            return View("Success");
        }

        // The UsePolicyServerClaimsTransformation middleware, maps roles to clams 
        // enabling you to use the classic authorize attribute
        [Authorize(Roles = "waitress")]
        public async Task<IActionResult> WaitressOnly()
        {
            // You can also use the IPolicyServerClient for the current authencated user to see if they 
            // have the role in question. This is the same as doing [Authorize(Roles = "waitress")]
            var isWaitress = await _client.IsInRoleAsync(User, "waitress");
            if (!isWaitress) return Forbid();

            return View("Success");
        }

        // The UsePolicyServerClaimsTransformation middleware, maps roles to clams 
        // enabling you to use the classic authorize attribute
        [Authorize(Roles = "patron")]
        public async Task<IActionResult> CustomerOnly()
        {
            // You can also use the IPolicyServerClient for the current authencated user to see if they 
            // have the role in question.  This is the same as doing [Authorize(Roles = "patron")]
            var isPatron = await _client.IsInRoleAsync(User, "patron");
            if (!isPatron) return Forbid();
            
            return View("Success");
        }

        #endregion

        #region Permission based actions.

        // The AuthorizationPermissionPolicies service, policy names are automatically mapped to permissions
        [Authorize("fillorder")]
        public async Task<IActionResult> PrepareOrder(string item, int amount)
        {
            var requirement = new MealRequirment
            {
                Amount = amount,
                Item = item
            };

            var result = await _auth.AuthorizeAsync(User, null, requirement);
            if (!result.Succeeded) return Forbid();

            return View("Success");
        }

        [Authorize("placeorder")]
        public async Task<IActionResult> OrderMeal(string item, int amount)
        {

            var requirement = new MealRequirment
            {
                Amount = amount,
                Item = item
            };

            var result = await _auth.AuthorizeAsync(User, null, requirement);
            if (!result.Succeeded) return Forbid();

            // or imperatively
            var canDiscipline = await _client.HasPermissionAsync(User, "placeorder");
            if (!canDiscipline) return Forbid();
            

            return View("Success");
        }

        [Authorize("serveorder")]
        public async Task<IActionResult> ServeMeal(string name, int amount)
        {
            // We can check the cucrent user imperatively to see if they have the permission to serveorder
            var canDiscipline = await _client.HasPermissionAsync(User, "serveorder");
            if (!canDiscipline) return Forbid();

            return View("Success");
        }

        #endregion
    }
}
