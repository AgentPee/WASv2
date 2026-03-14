using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WASv2.Data;
using static WASv2.Models.PRStatus;
using WASv2.Models;
using WASv2.Services;
using System.ComponentModel.DataAnnotations;

namespace WASv2.Controllers
{
    public class DepartmentHeadController : Controller
    {
        private readonly IPRService _prService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DepartmentHeadController(IPRService prService, ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _prService = prService;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var pendingPRs = _prService.GetPendingPRsForDepartmentHead();

            var viewModel = new PendingPRViewModel
            {
                PendingPRs = pendingPRs,
                TotalPending = pendingPRs.Count,
                TotalApproved = _prService.GetPRsByStatus(PRStatus.ApprovedByDepartmentHead).Count,
                TotalDisapproved = _prService.GetPRsByStatus(PRStatus.DisapprovedByDepartmentHead).Count
            };

            return View(viewModel);
        }

        //moved to index method
        /**
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
        **/

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
        public async Task<IActionResult> ApprovePR(string prNumber, string remarks, string reviewedBy)
        {
            // Fall back to identity name if reviewedBy not passed from form
            var approver = !string.IsNullOrEmpty(reviewedBy)
                ? reviewedBy
                : (User.Identity?.Name ?? "Department Head");

            var result = _prService.DepartmentHeadApprovePR(prNumber, approver, remarks);

            if (result)
            {
                TempData["SuccessMessage"] = $"PR #{prNumber} has been approved and forwarded to the Director.";
                Console.WriteLine($"============PR #{prNumber} APPROVED BY {approver} AND FORWARDED TO DIRECTOR============");
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to approve PR #{prNumber}. PR may not be in pending status.";
                Console.WriteLine($"==========FAILED TO APPROVE PR #{prNumber}============");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DisapprovePR(string prNumber, string remarks)
        {
            try
            {
                var reviewedBy = User.Identity.Name ?? "Department Head";

                var pr = _prService.GetPRByNumber(prNumber);

                if (pr == null)
                {
                    TempData["ErrorMessage"] = $"PR #{prNumber} not found.";
                    return RedirectToAction("Index");
                }

                var requestorId = pr.RequestedById;
                var requestorName = pr.RequestedBy;

                var result = _prService.DisapprovePR(prNumber, reviewedBy, remarks);

                if (result)
                {
                    Console.WriteLine($"PR #{prNumber} disapproved by {reviewedBy}. Remarks: {remarks}");


                    TempData["SuccessMessage"] = $"PR #{prNumber} has been disapproved. The requestor ({requestorName}) has been notified.";

                    return RedirectToAction("DisapprovedPRs");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to disapprove PR #{prNumber}. The PR may not be in a pending state.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disapproving PR: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred while disapproving the PR.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DisapprovedPRs()
        {
            var disapprovedPRs = _prService.GetPRsByStatus(PRStatus.DisapprovedByDepartmentHead);
            return View(disapprovedPRs);
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


            //For demo purposes, create a simple text file
            string content = GeneratePRFContent(pr);
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(fileBytes, "text/plain", $"PRF_{pr.PRNumber}.txt");

        }

        [HttpPost]
        public IActionResult ForwardToDirector(string prNumber)
        {
            var result = _prService.ForwardToDirector(prNumber);
            if (result)
            {
                TempData["SuccessMessage"] = $"PR #{prNumber} forwarded to Director for final approval.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to forward PR #{prNumber}.";
            }
            return RedirectToAction("PendingPR");
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