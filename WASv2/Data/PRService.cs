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

        public bool DepartmentHeadApprovePR(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.Pending)
            {
                pr.Status = PRStatus.PendingDirectorApproval;
                pr.ReviewedDate = DateTime.Now;
                pr.ReviewedBy = reviewedBy;
                pr.ApprovalRemarks = remarks;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public List<PRModel> GetPRsForDirector()
        {
            return _context.PRs
                .Include(p => p.Items)
                .Where(p => p.Status == PRStatus.PendingDirectorApproval)
                .OrderByDescending(p => p.SubmittedDate)
                .ToList();
        }

        public bool ForwardToDirector(string prNumber)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.Approved)
            {
                pr.Status = PRStatus.PendingDirectorApproval;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DirectorApprove(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.PendingDirectorApproval)
            {
                pr.Status = PRStatus.DirectorApproved;
                pr.ReviewedDate = DateTime.Now;
                pr.ReviewedBy = reviewedBy;
                pr.ApprovalRemarks = remarks;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DirectorReject(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.PendingDirectorApproval)
            {
                pr.Status = PRStatus.DirectorRejected;
                pr.ReviewedDate = DateTime.Now;
                pr.ReviewedBy = reviewedBy;
                pr.ApprovalRemarks = remarks;
                _context.SaveChanges();
                return true;
            }
            return false;
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
                _context.PRs.Add(prModel);
                int result = _context.SaveChanges();
                Console.WriteLine($"SaveChanges result: {result}");
                return result > 0 ? prModel : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreatePR: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
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