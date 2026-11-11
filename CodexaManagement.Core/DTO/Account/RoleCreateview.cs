using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CodexaManagement.Model.Enum;
using System.Text;

namespace CodexaManagement.Core.DTO.Account
{
    public class RoleCreateview
    {
        [Required]
        [RegularExpression(@"^[A-Za-z ]+$")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[A-Za-z ]+$")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please select a department.")]
        public DepartmentEnum? Department { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "User";
    }
}
