using CodexaManagement.Core.IRepositories;
using CodexaManagement.Core.IServices;
using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.Services
{
    public class EmployeeService : IEmployeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(
            IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetEmployeesAllAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetEmployeeByIdAsync(id);
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
        {
            return await _employeeRepository.GetEmployeeByUserIdAsync(userId);
        }
        public async Task CreateEmployeeAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.IdentityUserId))
            {
                throw new ArgumentException(
                    "An Identity user must be selected.");
            }

            if (await _employeeRepository.ExistsForUserAsync(
                    employee.IdentityUserId))
            {
                throw new InvalidOperationException(
                    "An employee record already exists for this user.");
            }

            await _employeeRepository.AddEmployeeAsync(employee);
            await _employeeRepository.SaveChangesAsync();
        }


        public async Task<Employee?> GetEmployeeByIdentityUserIdAsync(
     string identityUserId)
        {
            return await _employeeRepository
                .GetEmployeeByIdentityUserIdAsync(identityUserId);
        }

        public async Task<bool> UpdateMyDetailsAsync(
     string userId,
     string? phone,
     string? department,
     string location,
     DateTime dob,
     Gender gender)
        {
            var employee =
                await _employeeRepository.GetEmployeeByIdentityUserIdAsync(userId);

            if (employee == null)
                return false;

            employee.Phone = phone;
            employee.Department = department;
            employee.Location = location;
            employee.DOB = dob;
            employee.Gender = gender;

            return await _employeeRepository.UpdateMyDetailsAsync(employee);
        }
        public async Task<bool> UpdateSalaryAsync(
            int employeeId,
            decimal salary)
        {
            if (salary <= 1000)
                return false;

            return await _employeeRepository.UpdateSalaryAsync(
                employeeId,
                salary);
        }


        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            var existingEmployee =
                await _employeeRepository.GetEmployeeByIdAsync(employee.Id);

            if (existingEmployee == null)
                return false;

            existingEmployee.Phone = employee.Phone;
            existingEmployee.Department = employee.Department;
            existingEmployee.Location = employee.Location;
            existingEmployee.DOB = employee.DOB;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.UpdatedTime = DateTime.Now;

            await _employeeRepository.UpdateEmployeeAsync(existingEmployee);
            await _employeeRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee =
                await _employeeRepository.GetEmployeeByIdAsync(id);

            if (employee == null)
                return false;

            await _employeeRepository.DeleteAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return true;
        }

        
    }
}
