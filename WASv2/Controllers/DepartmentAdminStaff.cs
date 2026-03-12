using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using WASv2.Data;
using WASv2.Models;
using System.Linq;

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

            // If file doesn't exist, generate a sample file (for demo purposes)
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
            Console.WriteLine("=== SubmitToDeptHead Started ===");
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
                    //Console.WriteLine($"existing PR is null {existingPR = null}");
                    //if (existingPR != null) Console.WriteLine($"existingPR Id: {existingPR.Id}");

                    var prModel = new PRModel();
                    //var prModel = new PRModel
                    //{
                        //PRNumber = model.PRNumber,
                        //Id = existingPR?.Id ?? 0,
                        //Department = model.Department,
                        //RequestDate = model.RequestDate,
                        //RequestedBy = model.RequestedBy,
                        //RequestedById = GetCurrentUserId(),
                        //Purpose = model.Purpose,
                        //BudgetLine = model.BudgetLine,
                        //TotalAmount = model.TotalAmount,
                        //BudgetConfirmation = model.BudgetConfirmation,
                        //PRFFileName = fileName,
                        //PRFFilePath = $"/prf-files/{fileName}",
                        //Remarks = model.Remarks,
                        //Status = PRStatus.PendingDepartmentHeadApproval,
                        //SubmittedDate = DateTime.Now,
                        //Items = model.Items?.Select(i => new PRItemModel
                        //{
                        //    ItemNo = i.ItemNo,
                        //    Description = i.Description,
                        //    Quantity = i.Quantity,
                        //    Unit = i.Unit,
                        //    UnitPrice = i.UnitPrice,
                        //    TotalPrice = i.TotalPrice
                        //}).ToList() ?? new List<PRItemModel>()
                    //};
                    

                    Console.WriteLine("Calling PRService.CreatePR...");
                    //var savedPR = _prService.CreatePR(prModel);
                    //Console.WriteLine($"CreatePR result: {(savedPR != null ? "Success" : "Failed")}");

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

                    //if (savedPR != null)
                    //{
                    //    TempData["SuccessMessage"] = $"PR #{model.PRNumber} has been submitted to Department Head successfully!";
                    //    return RedirectToAction("Index");
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Failed to submit PR. Please try again.");
                    //}
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            Console.WriteLine("Returning to Index view");
            model.IsSearched = true;
            model.IsFound = true;
            return View("Index", model);
        }

        private async Task<string> SavePRFFile(IFormFile file, string prNumber)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "prf-files");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = $"PRF_{prNumber}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private int GetCurrentUserId()
        {
            // You'll need to set this up based on your authentication
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // In production, you should handle this better
            return 1;
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
            BUDGET CONFIRMATION: {prf.BudgetConfirmation}

            ITEMS:
            --------------------------------------------------\n";

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
            // First check if this PR already exists in the system using the service
            var existingPR = _prService.GetPRByNumber(prNumber);
            if (existingPR != null)
            {
                return new PRFViewModel
                {
                    PRNumber = existingPR.PRNumber,
                    Department = existingPR.Department,
                    RequestDate = existingPR.RequestDate,
                    RequestedBy = existingPR.RequestedBy,
                    Purpose = existingPR.Purpose,
                    BudgetLine = existingPR.BudgetLine,
                    TotalAmount = existingPR.TotalAmount,
                    BudgetConfirmation = existingPR.BudgetConfirmation,
                    PRFFileName = existingPR.PRFFileName,
                    Remarks = existingPR.Remarks,
                    Items = existingPR.Items?.Select(i => new PRFItemViewModel
                    {
                        ItemNo = i.ItemNo,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                };
            }

            // If not found in database, use sample data (for demo/pre-population)
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
                    TotalAmount = 500000.00M,
                    BudgetConfirmation = "Confirmed (Stock Replenishment)",
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
                    BudgetConfirmation = "Subject for Approval",
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

            // Also log ModelState errors
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
                        RequestedById = GetCurrentUserId(),
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