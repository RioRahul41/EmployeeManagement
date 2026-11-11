using CodexaManagement.Core.IRepositories;
using CodexaManagement.Data.DbContext;
using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using Microsoft.EntityFrameworkCore;


namespace CodexaManagement.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddEmployeeAsync(Employee employee)
        {
            await _context.AddAsync(employee);
        }

        public Task DeleteAsync(Employee employee)
        {
            employee.EmpIsDeleted = true;
            _context.Employees.Update(employee);
            return Task.CompletedTask;

        }

        public async Task<bool> ExistsForUserAsync(
             string identityUserId)
        {
            return await _context.Employees
                .AnyAsync(e => e.IdentityUserId == identityUserId);
        }


        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.EmpIsDeleted);
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.IdentityUserId == userId &&
                    !e.EmpIsDeleted);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAllAsync()
        {
            return await _context.Employees
                .Where(e => !e.EmpIsDeleted)
                .OrderByDescending(e=>e.CreatedTime)
                .ToListAsync();
        }
        public async Task<Employee?> GetEmployeeByIdentityUserIdAsync(
     string identityUserId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.IdentityUserId == identityUserId &&
                    !e.EmpIsDeleted);
        }

        public async Task<bool> UpdateMyDetailsAsync(
     string identityUserId,
     string? phone,
     string department,
     string location,
     DateTime dob,
     Gender gender)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId);

            if (employee == null)
                return false;

            employee.Phone = phone;
            employee.Location = location;
            employee.Department = department;
            employee.DOB = dob;
            employee.Gender = gender;
            employee.UpdatedTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> UpdateSalaryAsync(
            int employeeId,
            decimal salary)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.Id == employeeId &&
                    !e.EmpIsDeleted);

            if (employee == null)
                return false;

            // Update only salary.
            employee.Salary = salary;
            employee.UpdatedTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task SaveChangesAsync()
        {
           await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateMyDetailsAsync(Employee employee)
        {
            _context.Employees.Update(employee);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
