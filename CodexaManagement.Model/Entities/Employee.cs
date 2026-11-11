using CodexaManagement.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CodexaManagement.Model.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "First name can contain only letters and spaces.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Last name can contain only letters and spaces.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(
    @"^[0-9]{10}$",
    ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string? Phone { get; set; }
        [StringLength(100)]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public DateTime? UpdatedTime { get; set; }
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Employee), nameof(ValidateDOB))]
        public DateTime? DOB { get; set; }
        [Range(typeof(decimal), "0.01", "999999999",
    ErrorMessage = "Salary must be greater than zero.")]
        public decimal? Salary { get; set; }

        public bool EmpIsDeleted { get; set; } = false;

        public bool EmpIsActive { get; set; } = false;
        [EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        // Identity User relationship
        public string? IdentityUserId { get; set; }

        [StringLength(100)]
        public string? Designation {  get; set; }
        public IdentityApplication? IdentityUser { get; set; }


        public static ValidationResult? ValidateDOB(DateTime? dob)
        {
            if (!dob.HasValue)
                return ValidationResult.Success;

            if (dob.Value.Date >= DateTime.Today)
            {
                return new ValidationResult(
                    "Date of birth must be before today.");
            }

            return ValidationResult.Success;
        }


    }
}
