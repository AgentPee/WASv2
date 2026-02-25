using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class DepartmentAdminStaff : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreatePR()
        {
            return View();
        }
    }
}
