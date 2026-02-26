using Microsoft.AspNetCore.Mvc;

namespace WASv2.Controllers
{
    public class TopManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
