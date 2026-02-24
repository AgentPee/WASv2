using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WASv2.Models;

namespace WASv2.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PurchaseRequest()
        {
            return View();
        }

        public IActionResult RFQModule()
        {
            return View();
        }

        public IActionResult PurchaseOrder()
        {
            return View();
        }
        
        public IActionResult Accreditation()
        {
            return View();
        }

        public IActionResult Delivery()
        {
            return View();
        }

        public IActionResult PaymentMemo()
        {
            return View();
        }

        public IActionResult CertificateAcceptance()
        {
            return View();
        }

        public IActionResult CreatePR()
        {
            return View();
        }

        public IActionResult Privacy() 
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
