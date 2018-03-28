using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daimto.PolicyServer.WebAppSample.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(userName)) return View();

            var claims = new List<Claim>();

            switch (userName.Trim().ToLower())
            {
                case "mac":
                    claims = new List<Claim>
                    {
                        new Claim("sub", "1"),
                        new Claim("name", "Mac"),
                    };
                    break;
                case "betty":
                    claims = new List<Claim>
                    {
                        new Claim("sub", "10"),
                        new Claim("name", "Betty"),
                    };
                    break;
                default:
                    claims = new List<Claim>
                    {
                        new Claim("sub", "999"),
                        new Claim("name", userName),
                        new Claim("behavior", "unruly"),
                        new Claim("role", "student")
                    };
                    break;
            }

            var id = new ClaimsIdentity(claims, "password", "name", "role");
            var p = new ClaimsPrincipal(id);

            await HttpContext.SignInAsync(p);
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        public IActionResult AccessDenied() => View();
    }
}