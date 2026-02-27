using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class ManagerDirectorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
