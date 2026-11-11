using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.IServices
{
    public interface IEmployeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();

        Task<Employee?> GetEmployeeByIdAsync(int id);

        Task<Employee?> GetEmployeeByUserIdAsync(string userId);

        Task CreateEmployeeAsync(Employee employee);

        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<Employee?> GetEmployeeByIdentityUserIdAsync(
        string identityUserId);
        Task<bool> UpdateMyDetailsAsync(
    string identityUserId,
    string? phone,
    string department,
    string location,
    DateTime dob,
    Gender gender);

        Task<bool> UpdateSalaryAsync(
            int employeeId,
            decimal salary);
        Task<bool> DeleteEmployeeAsync(int id);
    }
}
