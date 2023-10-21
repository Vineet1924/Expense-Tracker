using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Expense_Tracker.Models;


namespace Expense_Tracker.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            
            if(claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Create", "Transaction");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(VMLogin user)
        {
            if(user.email == "user@example.com" && user.password == "123456")
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.email),
                };

                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = user.keepLoggedIn,
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authenticationProperties);

                return RedirectToAction("Create", "Transaction");
            }

            ViewData["ValidateMessage"] = "User not found";
            return View();
        }
    }
}
