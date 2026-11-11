using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CodexaManagement.Model.Entities;

namespace CodexaManagement.Core.IRepositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetEmployeesAllAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByUserIdAsync(string userId);
        Task<bool> ExistsForUserAsync(string identityUserId);

        Task AddEmployeeAsync(Employee employee);

        Task<Employee?> GetEmployeeByIdentityUserIdAsync(
    string identityUserId);

        Task<bool> UpdateMyDetailsAsync(Employee employee);


        Task<bool> UpdateSalaryAsync(
            int employeeId,
            decimal salary);

        Task UpdateEmployeeAsync(Employee employee);

        Task DeleteAsync(Employee employee);

        Task SaveChangesAsync();
    }
}
