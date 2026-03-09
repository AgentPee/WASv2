using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WASv2.Data;
using WASv2.Models;
using WASv2.Services;

namespace WASv2.Controllers
{
    public class DepartmentHeadController : Controller
    {
        private readonly IPRService _prService;

        public DepartmentHeadController(IPRService prService)
        {
            _prService = prService;
        }

        public IActionResult Index()
        {
            // Get counts for dashboard
            var pendingPRs = _prService.GetPRsByStatus(PRStatus.Pending);
            var approvedPRs = _prService.GetPRsByStatus(PRStatus.Approved);

            ViewBag.PendingCount = pendingPRs.Count;
            ViewBag.ApprovedCount = approvedPRs.Count;
            ViewBag.TotalAmount = pendingPRs.Sum(p => p.TotalAmount);

            return View();
        }

        public IActionResult PendingPR()
        {
            var pendingPRs = _prService.GetPendingPRsForDepartmentHead();

            var viewModel = new PendingPRViewModel
            {
                PendingPRs = pendingPRs,
                TotalPending = pendingPRs.Count,
                TotalApproved = _prService.GetPRsByStatus(PRStatus.Approved).Count,
                TotalDisapproved = _prService.GetPRsByStatus(PRStatus.Disapproved).Count
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ReviewPR(string prNumber)
        {
            if (string.IsNullOrEmpty(prNumber))
            {
                return RedirectToAction("PendingPR");
            }

            var pr = _prService.GetPRByNumber(prNumber);

            if (pr == null)
            {
                TempData["ErrorMessage"] = $"PR #{prNumber} not found.";
                return RedirectToAction("PendingPR");
            }

            return View(pr);
        }

        [HttpPost]
        public IActionResult ApprovePR(string prNumber, string remarks)
        {
            var reviewedBy = User.Identity.Name ?? "Department Head"; // Get actual user name
            var result = _prService.ApprovePR(prNumber, reviewedBy, remarks);

            if (result)
            {
                TempData["SuccessMessage"] = $"PR #{prNumber} has been approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to approve PR #{prNumber}.";
            }

            return RedirectToAction("PendingPR");
        }

        [HttpPost]
        public IActionResult DisapprovePR(string prNumber, string remarks)
        {
            var reviewedBy = User.Identity.Name ?? "Department Head"; // Get actual user name
            var result = _prService.DisapprovePR(prNumber, reviewedBy, remarks);

            if (result)
            {
                TempData["SuccessMessage"] = $"PR #{prNumber} has been disapproved.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to disapprove PR #{prNumber}.";
            }

            return RedirectToAction("PendingPR");
        }

        public IActionResult PaymentMemoReview()
        {
            return View();
        }

        public IActionResult CARating()
        {
            return View();
        }

        public IActionResult DownloadPRF(string prNumber)
        {
            var pr = _prService.GetPRByNumber(prNumber);

            if (pr == null || string.IsNullOrEmpty(pr.PRFFileName))
            {
                return NotFound();
            }

            // For demo purposes, create a simple text file
            string content = GeneratePRFContent(pr);
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(fileBytes, "text/plain", $"PRF_{pr.PRNumber}.txt");
        }

        private string GeneratePRFContent(PRModel pr)
        {
            string content = $@"
            DEPARTMENT HEAD REVIEW
            ======================
            PR NUMBER: {pr.PRNumber}
            DATE SUBMITTED: {pr.SubmittedDate:MM/dd/yyyy}
            DEPARTMENT: {pr.Department}
            REQUESTED BY: {pr.RequestedBy}
            PURPOSE: {pr.Purpose}
            BUDGET LINE: {pr.BudgetLine}
            BUDGET CONFIRMATION: {pr.BudgetConfirmation}

            ITEMS:
            --------------------------------------------------
            ";
                        foreach (var item in pr.Items)
                        {
                            content += $"{item.ItemNo}. {item.Description}\n";
                            content += $"   Quantity: {item.Quantity} {item.Unit}\n";
                            content += $"   Unit Price: {item.UnitPrice:C2}\n";
                            content += $"   Total: {item.TotalPrice:C2}\n\n";
                        }

                        content += $@"
            --------------------------------------------------
            TOTAL AMOUNT: {pr.TotalAmount:C2}

            REQUESTOR REMARKS: {pr.Remarks}
            ";

            return content;
        }
    }
}