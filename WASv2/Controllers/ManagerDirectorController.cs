using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class ManagerDirectorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DirectorPReview()
        {
            return View();
        }
    }
}
