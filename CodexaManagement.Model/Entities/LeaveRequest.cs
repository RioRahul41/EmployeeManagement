
using CodexaManagement.Model.Enum;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodexaManagement.Model.Entities
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        // Employee who applied for leave
        [Required]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalDays { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        // Uploaded document information
        public string? DocumentPath { get; set; }

        public string? OriginalFileName { get; set; }

        // Pending, Approved or Rejected
        [Required]
        public LeaveRequestStatus Status { get; set; }
    = LeaveRequestStatus.Pending;

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        // Identity user ID of HR who processes the request
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? HRRemarks { get; set; }
    }
}
