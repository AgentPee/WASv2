using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WASv2.Models;

namespace WASv2.Controllers
{
    public class DepartmentHeadController : Controller
    {
        public IActionResult PendingPR()
        {
            return View();
        }
    }
}
