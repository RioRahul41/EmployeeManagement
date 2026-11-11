using System.ComponentModel.DataAnnotations;

namespace CodexaManagement.Web.Models;

public class ApplyLeaveViewModel
{
    [Required, StringLength(30)]
    [Display(Name = "Leave type")]
    public string LeaveType { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime? EndDate { get; set; }

    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Proof document")]
    public IFormFile? ProofDocument { get; set; }
}

public class ReviewLeaveViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int WorkingDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProofFileName { get; set; }

    [StringLength(1000)]
    [Display(Name = "HR comment")]
    public string? ReviewComment { get; set; }
}
