using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WASv2.Data;
using System.Threading.Tasks;
using WASv2.Services;
using System.Collections.Generic;
using WASv2.Helpers; // Add this

namespace WASv2.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMyDbService _myDbService;

        public AuthController(IMyDbService myDbService)
        {
            _myDbService = myDbService;
        }

        public IActionResult Index() => View();
        public IActionResult SignIn() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _myDbService.ValidateUser(model.Email, model.PasswordHash);

                if (user != null)
                {
                    int roleId = user.RoleID ?? 0;

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, roleId.ToString()),
                        // Optional: Add role name claim for easier display
                        new Claim("RoleName", RoleHelpers.GetRoleName(roleId))
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    // Use RoleHelpers to determine redirect
                    var controllerName = RoleHelpers.GetDashboardController(roleId);
                    return RedirectToAction("Index", controllerName);
                }
                ModelState.AddModelError("", "Invalid Login Attempt.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult SignUp() => View();
        public IActionResult AccessDenied() => View();
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        public bool RememberMe { get; set; }
    }
}