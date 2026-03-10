using Microsoft.AspNetCore.Mvc;
using WASv2.Data;
using WASv2.Models;
using System.Linq;

namespace WASv2.Controllers
{
    public class ManagerDirectorController : Controller
    {
        private readonly IPRService _prService;

        public ManagerDirectorController(IPRService prService)
        {
            _prService = prService;
        }

        public IActionResult Index()
        {
            var pendingPRs = _prService.GetPRsForDirector();
            ViewBag.PendingCount = pendingPRs.Count;
            ViewBag.TotalAmount = pendingPRs.Sum(p => p.TotalAmount);
            return View(pendingPRs);
        }


        public IActionResult DirectorPReview(string prNumber)
        {
            if (string.IsNullOrEmpty(prNumber))
                return RedirectToAction("Index");

            var pr = _prService.GetPRByNumber(prNumber);
            if (pr == null)
            {
                TempData["ErrorMessage"] = "PR not found.";
                return RedirectToAction("Index");
            }
            return View(pr);
        }

        [HttpPost]
        public IActionResult ApprovePR(string prNumber, string remarks)
        {
            var reviewedBy = User.Identity.Name ?? "Director";
            var result = _prService.DirectorApprove(prNumber, reviewedBy, remarks);
            if (result)
                TempData["SuccessMessage"] = $"PR #{prNumber} approved and forwarded to Purchasing.";
            else
                TempData["ErrorMessage"] = $"Failed to approve PR #{prNumber}.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectPR(string prNumber, string remarks)
        {
            var reviewedBy = User.Identity.Name ?? "Director";
            var result = _prService.DirectorReject(prNumber, reviewedBy, remarks);
            if (result)
                TempData["SuccessMessage"] = $"PR #{prNumber} rejected and returned to Department Head.";
            else
                TempData["ErrorMessage"] = $"Failed to reject PR #{prNumber}.";
            return RedirectToAction("Index");
        }
    }
}