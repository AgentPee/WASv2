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
                .Where(p => p.Status == PRStatus.PendingDepartmentHeadApproval);

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(p => p.Department == department);
            }

            return query.OrderByDescending(p => p.SubmittedDate).ToList();
        }

        public bool DepartmentHeadApprovePR(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.PendingDepartmentHeadApproval)
            {
                pr.Status = PRStatus.PendingDirectorApproval;
                Console.WriteLine("succesfully updated the status");
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
            if (pr != null && pr.Status == PRStatus.ApprovedByDepartmentHead)
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
                pr.Status = PRStatus.ApprovedByDirector;
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
                pr.Status = PRStatus.RejectedByDirector;
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
                .AsNoTracking()
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
            prModel.Status = PRStatus.PendingDepartmentHeadApproval;
            prModel.SubmittedDate = DateTime.Now;
            _context.PRs.Add(prModel);
            _context.SaveChanges();
            return prModel;
        }

        public PRModel UpdatePR(PRModel prModel)
        {
            var existing = _context.PRs
        .Include(p => p.Items)
        .FirstOrDefault(p => p.Id == prModel.Id);
            if (existing == null) return null;

            existing.Department = prModel.Department;
            existing.RequestDate = prModel.RequestDate;
            existing.RequestedBy = prModel.RequestedBy;
            existing.Purpose = prModel.Purpose;
            existing.BudgetLine = prModel.BudgetLine;
            existing.TotalAmount = prModel.TotalAmount;
            existing.BudgetConfirmation = prModel.BudgetConfirmation;
            existing.PRFFileName = prModel.PRFFileName;
            existing.PRFFilePath = prModel.PRFFilePath;
            existing.Remarks = prModel.Remarks;
            existing.Status = prModel.Status;
            existing.SubmittedDate = prModel.SubmittedDate;

            _context.PRItems.RemoveRange(existing.Items);

            foreach (var item in prModel.Items)
            {
                item.Id = 0;
                item.PRId = existing.Id;
                existing.Items.Add(item);
            }

            _context.SaveChanges();
            return existing;
        }

        public bool ApprovePR(string prNumber, string reviewedBy, string remarks)
        {
            var pr = GetPRByNumber(prNumber);
            if (pr != null && pr.Status == PRStatus.PendingDepartmentHeadApproval)
            {
                pr.Status = PRStatus.ApprovedByDepartmentHead;
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
            if (pr != null && pr.Status == PRStatus.PendingDepartmentHeadApproval)
            {
                pr.Status = PRStatus.DisapprovedByDepartmentHead;
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
            if (pr != null && pr.Status == PRStatus.ApprovedByDepartmentHead)
            {
                pr.Status = PRStatus.ForwardedToPurchasing;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public List<PRModel> GetPRsByStatus(string status, string department = null)
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