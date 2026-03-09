using System;
using System.Collections.Generic;
using System.Linq;
using WASv2.Models;
using WASv2.Data;
using Microsoft.EntityFrameworkCore;

namespace WASv2.Data
{
    public class PRService : IPRService
    {
        private readonly ApplicationDbContext _context;

        public PRService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PRModel> GetPendingPRsForDepartmentHead(string department = null)
        {
            var query = _context.PRs
                .Include(p => p.Items)
                .Where(p => p.Status == PRStatus.Pending);

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(p => p.Department == department);
            }

            return query.OrderByDescending(p => p.SubmittedDate).ToList();
        }

        public PRModel GetPRByNumber(string prNumber)
        {
            return _context.PRs
                .Include(p => p.Items)
                .FirstOrDefault(p => p.PRNumber == prNumber);
        }

        public PRModel GetPRById(int id)
        {
            return _context.PRs
                .Include(p => p.Items)
                .FirstOrDefault(p => p.Id == id);
        }

        public PRModel CreatePR(PRModel prModel)
        {
            try
            {
                Console.WriteLine($"PRService.CreatePR - Starting for PR: {prModel.PRNumber}");

                prModel.Status = PRStatus.Pending;
                prModel.SubmittedDate = DateTime.Now;

                Console.WriteLine("Adding to context...");
                _context.PRs.Add(prModel);

                Console.WriteLine("Saving changes...");
                var result = _context.SaveChanges();
                Console.WriteLine($"SaveChanges result: {result} records saved");

                if (result > 0)
                {
                    Console.WriteLine($"PR saved successfully with ID: {prModel.Id}");
                    return prModel;
                }
                else
                {
                    Console.WriteLine("No records were saved");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreatePR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public bool ApprovePR(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.Pending)
            {
                pr.Status = PRStatus.Approved;
                pr.ReviewedDate = DateTime.Now;
                pr.ReviewedBy = reviewedBy;
                pr.ApprovalRemarks = remarks;

                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DisapprovePR(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.Pending)
            {
                pr.Status = PRStatus.Disapproved;
                pr.ReviewedDate = DateTime.Now;
                pr.ReviewedBy = reviewedBy;
                pr.ApprovalRemarks = remarks;

                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool ForwardToProcurement(string prNumber)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.Approved)
            {
                pr.Status = PRStatus.Forwarded;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public List<PRModel> GetPRsByStatus(PRStatus status, string department = null)
        {
            var query = _context.PRs
                .Include(p => p.Items)
                .Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(p => p.Department == department);
            }

            return query.ToList();
        }

        public List<PRModel> GetPRsByDepartment(string department)
        {
            return _context.PRs
                .Include(p => p.Items)
                .Where(p => p.Department == department)
                .OrderByDescending(p => p.SubmittedDate)
                .ToList();
        }


    }
}