using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WASv2.Controllers
{
    public class DepartmentAdminStaff : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public DepartmentAdminStaff(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var model = new PRFViewModel
            {
                IsSearched = false,
                BudgetConfirmation = "Confirmed (Stock Replenishment)"
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult SearchPR(PRFViewModel model)
        {
            if (string.IsNullOrEmpty(model.PRNumber))
            {
                ModelState.AddModelError("PRNumber", "Please enter a PR Number");
                model.IsSearched = true;
                model.IsFound = false;
                return View("Index", model);
            }

            // Simulate searching for PRF (replace with actual database call)
            var prf = GetPRFByNumber(model.PRNumber);

            if (prf != null)
            {
                model.IsSearched = true;
                model.IsFound = true;

                // Map the found PRF to the model
                model.Department = prf.Department;
                model.RequestDate = prf.RequestDate;
                model.RequestedBy = prf.RequestedBy;
                model.Purpose = prf.Purpose;
                model.BudgetLine = prf.BudgetLine;
                model.TotalAmount = prf.TotalAmount;
                model.Items = prf.Items;
                model.PRFFileName = prf.PRFFileName;
                model.Remarks = prf.Remarks;
            }
            else
            {
                model.IsSearched = true;
                model.IsFound = false;
                ModelState.AddModelError("PRNumber", "PR Number not found");
            }

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult DownloadPRF(string prNumber)
        {
            if (string.IsNullOrEmpty(prNumber))
            {
                return NotFound();
            }

            // Get PRF details (replace with actual database call)
            var prf = GetPRFByNumber(prNumber);

            if (prf == null || string.IsNullOrEmpty(prf.PRFFileName))
            {
                return NotFound();
            }

            // Construct the file path
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "prf-files");
            string filePath = Path.Combine(uploadsFolder, prf.PRFFileName);

            // If file doesn't exist, generate a sample file (for demo purposes)
            if (!System.IO.File.Exists(filePath))
            {
                return GenerateSamplePRF(prf);
            }

            // Read the file and return it
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string contentType = GetContentType(filePath);
            return File(fileBytes, contentType, prf.PRFFileName);
        }

        [HttpPost]
        public IActionResult SubmitToDeptHead(PRFViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Here you would save to database and send notification
                TempData["SuccessMessage"] = $"PR #{model.PRNumber} has been submitted to Department Head successfully!";
                return RedirectToAction("Index");
            }

            model.IsSearched = true;
            model.IsFound = true;
            return View("Index", model);
        }

        private IActionResult GenerateSamplePRF(PRFViewModel prf)
        {
            string content = $@"
            PRF NUMBER: {prf.PRNumber}
            DATE: {prf.RequestDate:MM/dd/yyyy}
            DEPARTMENT: {prf.Department}
            REQUESTED BY: {prf.RequestedBy}
            PURPOSE: {prf.Purpose}
            BUDGET LINE: {prf.BudgetLine}

            ITEMS:
            --------------------------------------------------
            ";
                        foreach (var item in prf.Items)
                        {
                            content += $"{item.ItemNo}. {item.Description} - {item.Quantity} {item.Unit} @ {item.UnitPrice:C2} = {item.TotalPrice:C2}\n";
                        }

                        content += $@"
            --------------------------------------------------
            TOTAL AMOUNT: {prf.TotalAmount:C2}

            REMARKS: {prf.Remarks}
            ";

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(fileBytes, "text/plain", $"PRF_{prf.PRNumber}.txt");
        }

        // Helper method to determine content type
        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        private PRFViewModel GetPRFByNumber(string prNumber)
        {
            // This is sample data - replace with actual database query
            var samplePRFs = new Dictionary<string, PRFViewModel>
            {
                ["PR-2026-001"] = new PRFViewModel
                {
                    PRNumber = "PR-2026-001",
                    Department = "Information Technology",
                    RequestDate = DateTime.Now.AddDays(-5),
                    RequestedBy = "John Doe",
                    Purpose = "Hardware Replacement for Q2",
                    BudgetLine = "IT Equipment - CAPEX",
                    TotalAmount = 150000.00M,
                    PRFFileName = "PR-2026-001_Hardware_Request.pdf",
                    Remarks = "Urgent: Need for new developers",
                    Items = new List<PRFItemViewModel>
                    {
                        new PRFItemViewModel { ItemNo = 1, Description = "Dell XPS Laptops", Quantity = 5, Unit = "pcs", UnitPrice = 85000.00M, TotalPrice = 425000.00M },
                        new PRFItemViewModel { ItemNo = 2, Description = "Dell Monitors 27\"", Quantity = 5, Unit = "pcs", UnitPrice = 15000.00M, TotalPrice = 75000.00M }
                    }
                },
                ["PR-2026-002"] = new PRFViewModel
                {
                    PRNumber = "PR-2026-002",
                    Department = "Human Resources",
                    RequestDate = DateTime.Now.AddDays(-3),
                    RequestedBy = "Jane Smith",
                    Purpose = "Training Materials",
                    BudgetLine = "Training - OPEX",
                    TotalAmount = 50000.00M,
                    PRFFileName = "PR-2026-002_Training_Materials.pdf",
                    Remarks = "For Q2 Training",
                    Items = new List<PRFItemViewModel>
                    {
                        new PRFItemViewModel { ItemNo = 1, Description = "Training Manuals", Quantity = 50, Unit = "pcs", UnitPrice = 500.00M, TotalPrice = 25000.00M },
                        new PRFItemViewModel { ItemNo = 2, Description = "Training Kits", Quantity = 50, Unit = "sets", UnitPrice = 500.00M, TotalPrice = 25000.00M }
                    }
                }
            };

            return samplePRFs.ContainsKey(prNumber) ? samplePRFs[prNumber] : null;
        }

        public IActionResult CreatePR()
        {
            return View();
        }
    }



    public class PRFViewModel
    {
        [Required]
        [Display(Name = "PR Number")]
        public string PRNumber { get; set; }

        public bool IsSearched { get; set; }
        public bool IsFound { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [Display(Name = "Budget Line")]
        public string BudgetLine { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public List<PRFItemViewModel> Items { get; set; }

        [Display(Name = "Budget Confirmation")]
        public string BudgetConfirmation { get; set; }

        [Display(Name = "PRF File")]
        public string PRFFileName { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }

    public class PRFItemViewModel
    {
        public int ItemNo { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
