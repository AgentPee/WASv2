using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WASv2.Data;
using System.Threading.Tasks;
using WASv2.Services;

namespace WASv2.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMyDbService _myDbService;
        private object MyDBService;
        private BinaryReader Name;
        private BinaryReader Email;
        private BinaryReader Role;
        private BinaryReader DeptId;
        private const string DeptId = "DepartmentId";

        public AuthController(IMyDbService myDbService)
        {
            _myDbService = myDbService;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await MyDBService. ValidateUser(model.Email, model.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    //Role-Based Redirection
                    return user.RoleName switch
                    {
                        "Supplier" => RedirectToAction("", ""),
                        "DeptHead" => RedirectToAction("", ""),
                        "Purchasing" => RedirectToAction("", ""),
                        _ => RedirectToAction("", "")
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

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
