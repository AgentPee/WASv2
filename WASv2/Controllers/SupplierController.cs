using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class SupplierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SupplierPortal()
        {
            return View();
        }
    }
}
