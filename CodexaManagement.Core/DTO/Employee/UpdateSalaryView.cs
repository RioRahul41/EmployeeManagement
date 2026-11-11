using CodexaManagement.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CodexaManagement.Core.DTO.Employee
{
    public class UpdateSalaryView
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(typeof(decimal), "0.99", "999999999",
            ErrorMessage = "Salary must be greater than zero.")]

        public decimal? Salary { get; set; }
    }
}
