using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
