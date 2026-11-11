
using CodexaManagement.Model.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace CodexaManagement.Core.DTO.Employee
{
    public class UpdateMyDetailsView
    {
        public string IdentityUserId { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Phone must contain exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public Gender? Gender { get; set; }
    }
}
