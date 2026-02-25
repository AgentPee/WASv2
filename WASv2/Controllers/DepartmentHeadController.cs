using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WASv2.Models;

namespace WASv2.Controllers
{
    public class DepartmentHeadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult PendingPR()
        {
            return View();
        }

        public IActionResult PaymentMemoReview()
        {
            return View();
        }

        public IActionResult CARating()
        {
            return View();
        }
    }
}