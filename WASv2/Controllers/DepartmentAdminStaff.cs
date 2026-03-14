using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WASv2.Data;
using WASv2.Models;

namespace WASv2.Controllers
{
    public class DepartmentAdminStaff : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IPRService _prService;

        public DepartmentAdminStaff(IWebHostEnvironment hostEnvironment, IPRService prService)
        {
            _hostEnvironment = hostEnvironment;
            _prService = prService;
        }

        public IActionResult Index()
        {
            var model = new PRFViewModel
            {
                IsSearched = false,
                BudgetConfirmation = "Confirmed (Stock Replenishment)"
            };

            // Pass approved/disapproved PRs to view
            ViewBag.ApprovedPRs = _prService.GetPRsByStatus(PRStatus.ApprovedByDepartmentHead);
            ViewBag.DisapprovedPRs = _prService.GetPRsByStatus(PRStatus.DisapprovedByDepartmentHead);

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

            var prf = GetPRFByNumber(model.PRNumber);

            if (prf != null)
            {
                model.IsSearched = true;
                model.IsFound = true;

                model.Department = prf.Department;
                model.RequestDate = prf.RequestDate;
                model.RequestedBy = prf.RequestedBy;
                model.Purpose = prf.Purpose;
                model.BudgetLine = prf.BudgetLine;
                model.TotalAmount = prf.TotalAmount;
                model.Items = prf.Items;
                model.PRFFileName = prf.PRFFileName;
                model.Remarks = prf.Remarks;
                model.BudgetConfirmation = prf.BudgetConfirmation;
            }
            else
            {
                model.IsSearched = true;
                model.IsFound = false;
                ModelState.AddModelError("PRNumber", "PR Number not found");
            }

            // Re-populate ViewBag so sidebar still renders after search
            ViewBag.ApprovedPRs = _prService.GetPRsByStatus(PRStatus.ApprovedByDepartmentHead);
            ViewBag.DisapprovedPRs = _prService.GetPRsByStatus(PRStatus.DisapprovedByDepartmentHead);

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult DownloadPRF(string prNumber)
        {
            if (string.IsNullOrEmpty(prNumber))
            {
                return NotFound();
            }

            var prf = GetPRFByNumber(prNumber);

            if (prf == null || string.IsNullOrEmpty(prf.PRFFileName))
            {
                return NotFound();
            }

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "prf-files");
            string filePath = Path.Combine(uploadsFolder, prf.PRFFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return GenerateSamplePRF(prf);
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string contentType = GetContentType(filePath);
            return File(fileBytes, contentType, prf.PRFFileName);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitToDeptHead(PRFViewModel model, IFormFile? prfFile)
        {
            Console.WriteLine("============SUBMIT TO DEPT HEAD===========");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            Console.WriteLine($"PR Number: {model.PRNumber}");
            Console.WriteLine($"File received: {(prfFile != null ? prfFile.FileName : "No file")}");

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("ModelState is valid, processing submission...");

                    string fileName = model.PRFFileName;
                    if (prfFile != null && prfFile.Length > 0)
                    {
                        Console.WriteLine($"Saving file: {prfFile.FileName}");
                        fileName = await SavePRFFile(prfFile, model.PRNumber);
                        Console.WriteLine($"File saved as: {fileName}");
                    }

                    var existingPR = _prService.GetPRByNumber(model.PRNumber);

                    var prModel = new PRModel();

                    Console.WriteLine("Calling PRService.CreatePR...");

                    Console.WriteLine($"existingPR is null? {existingPR == null}");
                    if (existingPR != null)
                    {
                        Console.WriteLine($"existingPR Id: {existingPR.Id}");
                    }

                    if (existingPR != null)
                    {
                        existingPR.Department = model.Department;
                        existingPR.RequestDate = model.RequestDate;
                        existingPR.RequestedBy = model.RequestedBy;
                        existingPR.Purpose = model.Purpose;
                        existingPR.BudgetLine = model.BudgetLine;
                        existingPR.TotalAmount = model.TotalAmount;
                        existingPR.BudgetConfirmation = model.BudgetConfirmation;
                        existingPR.Remarks = model.Remarks;
                        existingPR.Status = PRStatus.PendingDepartmentHeadApproval;
                        existingPR.SubmittedDate = DateTime.Now;
                    }
                    else
                    {
                        _prService.CreatePR(prModel);
                        Console.WriteLine("PR created.");
                    }

                    TempData["SuccessMessage"] = $"PR #{model.PRNumber} submitted to Department Head successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error submitting PR: {ex.Message}");
                }
            }

            ViewBag.ApprovedPRs = _prService.GetPRsByStatus(PRStatus.ApprovedByDepartmentHead);
            ViewBag.DisapprovedPRs = _prService.GetPRsByStatus(PRStatus.DisapprovedByDepartmentHead);

            return View("Index", model);
        }

        public IActionResult CreatePR()
        {
            var model = new PRFViewModel
            {
                RequestDate = DateTime.Now,
                BudgetConfirmation = "Confirmed (Stock Replenishment)",
                Items = new List<PRFItemViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePR(PRFViewModel model, IFormFile prfFile)
        {
            Console.WriteLine("=== CreatePR POST ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"PRNumber: {model.PRNumber}");
            Console.WriteLine($"Items count: {model.Items?.Count ?? 0}");
            if (model.Items != null)
            {
                foreach (var item in model.Items)
                {
                    Console.WriteLine($"  Item {item.ItemNo}: {item.Description}, Qty: {item.Quantity}");
                }
            }
            else
            {
                Console.WriteLine("Items is null");
            }

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Validation error on {key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPR = _prService.GetPRByNumber(model.PRNumber);
                    if (existingPR != null)
                    {
                        ModelState.AddModelError("PRNumber", "PR Number already exists. Please use a different number.");
                        return View(model);
                    }

                    string fileName = null;
                    if (prfFile != null && prfFile.Length > 0)
                    {
                        fileName = await SavePRFFile(prfFile, model.PRNumber);
                    }

                    decimal totalAmount = model.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;

                    var prModel = new PRModel
                    {
                        PRNumber = model.PRNumber,
                        Department = model.Department,
                        RequestDate = model.RequestDate,
                        RequestedBy = model.RequestedBy,
                        //RequestedById = GetCurrentUserId(),
                        Purpose = model.Purpose,
                        BudgetLine = model.BudgetLine,
                        TotalAmount = totalAmount,
                        BudgetConfirmation = model.BudgetConfirmation,
                        PRFFileName = fileName,
                        PRFFilePath = fileName != null ? $"/prf-files/{fileName}" : null,
                        Remarks = model.Remarks,
                        Status = PRStatus.PendingDepartmentHeadApproval,
                        SubmittedDate = DateTime.Now,
                        Items = model.Items?.Select(i => new PRItemModel
                        {
                            ItemNo = i.ItemNo,
                            Description = i.Description,
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            UnitPrice = i.UnitPrice,
                            TotalPrice = i.Quantity * i.UnitPrice
                        }).ToList() ?? new List<PRItemModel>()
                    };

                    var savedPR = _prService.CreatePR(prModel);

                    if (savedPR != null)
                    {
                        TempData["SuccessMessage"] = $"PR #{model.PRNumber} has been created successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create PR. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating PR: {ex.Message}");
                }
            }

            return View(model);
        }

        // ── helpers (unchanged) ─────────────────────────────────────────────

        private string GetCurrentUserId() => User?.Identity?.Name ?? "Unknown";

        private async Task<string> SavePRFFile(IFormFile file, string prNumber)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "prf-files");
            Directory.CreateDirectory(uploadsFolder);
            string fileName = $"{prNumber}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private string GetContentType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private IActionResult GenerateSamplePRF(PRFViewModel prf)
        {
            string content = $"PR Number: {prf.PRNumber}\nDepartment: {prf.Department}\nRequested By: {prf.RequestedBy}";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", $"PRF_{prf.PRNumber}.txt");
        }

        private PRFViewModel GetPRFByNumber(string prNumber)
        {
            var pr = _prService.GetPRByNumber(prNumber);
            if (pr == null) return null;

            return new PRFViewModel
            {
                PRNumber = pr.PRNumber,
                Department = pr.Department,
                RequestDate = pr.RequestDate,
                RequestedBy = pr.RequestedBy,
                Purpose = pr.Purpose,
                BudgetLine = pr.BudgetLine,
                TotalAmount = pr.TotalAmount,
                BudgetConfirmation = pr.BudgetConfirmation,
                PRFFileName = pr.PRFFileName,
                Remarks = pr.Remarks,
                Items = pr.Items?.Select(i => new PRFItemViewModel
                {
                    ItemNo = i.ItemNo,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList() ?? new List<PRFItemViewModel>()
            };
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
        public string? PRFFileName { get; set; }

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