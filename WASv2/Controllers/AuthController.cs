using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WASv2.Data;
using System.Threading.Tasks;
using WASv2.Services;
using System.Collections.Generic;


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
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.RoleID.ToString()) 
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    
                    return user.RoleID switch
                    {
                        1 => RedirectToAction("Index", "Supplier"),
                        2 => RedirectToAction("Index", "DepartmentHead"),
                        3 => RedirectToAction("Index", "PurchasingOfficer"),
                        _ => RedirectToAction("Index", "Home")
                    };
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