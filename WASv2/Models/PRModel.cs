using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WASv2.Controllers;

namespace WASv2.Models
{
    public class PRModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "PR Number")]
        public string PRNumber { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Requested By ID")]
        public int RequestedById { get; set; }

        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [Display(Name = "Budget Line")]
        public string BudgetLine { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Budget Confirmation")]
        public string BudgetConfirmation { get; set; }

        [Display(Name = "PRF File Name")]
        public string? PRFFileName { get; set; }

        [Display(Name = "PRF File Path")]
        public string? PRFFilePath { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Status")]
        //public PRStatus Status { get; set; }
        public string Status { get; set; }

        [Display(Name = "Submitted Date")]
        public DateTime SubmittedDate { get; set; }

        [Display(Name = "Reviewed Date")]
        public DateTime? ReviewedDate { get; set; }

        [Display(Name = "Reviewed By")]
        public string? ReviewedBy { get; set; }

        [Display(Name = "Approval Remarks")]
        public string? ApprovalRemarks { get; set; }

        public List<PRItemModel> Items { get; set; }
        
    }

    public class PRItemModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PR")]
        public int PRId { get; set; }

        public int ItemNo { get; set; }

        [Required]
        public string Description { get; set; }

        public int Quantity { get; set; }

        public string Unit { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public PRModel PR { get; set; }
    }

    //public enum PRStatus
    public static class PRStatus
    {
        /**
        [Display(Name = "Pending Department Head Approval")]
        Pending = 1,

        [Display(Name = "Approved by Department Head")]
        Approved = 2,

        [Display(Name = "Disapproved by Department Head")]
        Disapproved = 3,

        [Display(Name = "Forwarded to Purchasing")]
        Forwarded = 4,

        [Display(Name = "Pending Director Approval")]
        PendingDirectorApproval = 5,

        [Display(Name = "Approved by Director")]
        DirectorApproved = 6,

        [Display(Name = "Rejected by Director")]
        DirectorRejected = 7
        **/

        public const string PendingDepartmentHeadApproval = "Pending Department Head Approval";
        public const string ApprovedByDepartmentHead = "Approved by Department Head";
        public const string DisapprovedByDepartmentHead = "Disapproved by Department Head";
        public const string ForwardedToPurchasing = "Forwarded to Purchasing";
        public const string PendingDirectorApproval = "Pending Director Approval";
        public const string ApprovedByDirector = "Approved by Director";
        public const string RejectedByDirector = "Rejected by Director";
    }

    public class PendingPRViewModel
    {
        public List<PRModel> PendingPRs { get; set; }
        public int TotalPending { get; set; }
        public int TotalApproved { get; set; }
        public int TotalDisapproved { get; set; }
    }
}