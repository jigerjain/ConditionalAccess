using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Okta.AspNetCore;

namespace ConditionalAccess.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult SignIn()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Challenge(OktaDefaults.MvcAuthenticationScheme);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult SignOut()
        {
            return new SignOutResult(
                new[]
                {
                     OktaDefaults.MvcAuthenticationScheme,
                     CookieAuthenticationDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "/Home/" });
        }
    }
}
