using System.ComponentModel.DataAnnotations;

namespace CodexaManagement.Web.Models
{
    public class EditLeaveViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Leave type is required.")]
        [StringLength(30)]
        [Display(Name = "Leave type")]
        public string LeaveType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Proof document")]
        public IFormFile? ProofDocument { get; set; }

        public string? ExistingProofFileName { get; set; }
    }
}
