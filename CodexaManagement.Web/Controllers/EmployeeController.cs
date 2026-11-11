
using CodexaManagement.Core.DTO.Employee;
using CodexaManagement.Core.IServices;
using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodexaManagement.Web.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeService _employeeService;
        private readonly UserManager<IdentityApplication> _userManager;

        public EmployeeController(
            IEmployeService employeeService,
            UserManager<IdentityApplication> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }

        // HR/Admin: Display all employees
        [Authorize(Roles = "HR,Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var employees =
                await _employeeService.GetAllEmployeesAsync();

            return View(employees);
        }

        // HR/Admin: Display one employee
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee =
                await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // HR/Admin: Display salary form
        [Authorize(Roles = "HR,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee =
                await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            var model = new UpdateSalaryView
            {
                Id = employee.Id,
                Salary = employee.Salary
            };

            return View(model);
        }

        // HR/Admin: Update ONLY salary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSalaryView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(model.Id);

            if (employee == null)
            {
                return NotFound();
            }

            employee.Salary = model.Salary;

            await _employeeService.UpdateEmployeeAsync(employee);

            TempData["Success"] = "Salary updated successfully.";

            return RedirectToAction("Index");
        }

        // HR/Admin: Show soft-delete confirmation
        [Authorize(Roles = "HR,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employee =
                await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // HR/Admin: Perform soft delete
        [Authorize(Roles = "HR,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted =
                await _employeeService.DeleteEmployeeAsync(id);

            if (!deleted)
                return NotFound();

            TempData["Success"] = "Employee deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Employee: Display their own profile form
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> UpdateMyDetails()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            var employee = await _employeeService
                .GetEmployeeByIdentityUserIdAsync(userId);

            if (employee == null)
                return NotFound("Employee profile was not found.");

            var model = new UpdateMyDetailsView
            {
                Phone = employee.Phone,
                Department = employee.Department,
                Location = employee.Location,
                DOB = employee.DOB,
                Gender = employee.Gender
            };

            return View(model);
        }

        // Employee: Update only their own permitted details

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMyDetails(UpdateMyDetailsView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // 1. Find the logged-in Identity user
            var identityUser = await _userManager.FindByIdAsync(userId);

            if (identityUser == null)
                return NotFound("User was not found.");

            // 2. Update phone number in AspNetUsers
            identityUser.PhoneNumber = model.Phone?.Trim();

            var identityResult = await _userManager.UpdateAsync(identityUser);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // 3. Update phone number and other details in Employees
            var result = await _employeeService.UpdateMyDetailsAsync(
                userId,
                model.Phone?.Trim(),
                model.Department,
                model.Location,
                model.DOB!.Value,
                model.Gender!.Value);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "The Identity phone number was updated, but the employee details could not be saved.");

                return View(model);
            }

            TempData["Success"] = "Your details were updated successfully.";

            return RedirectToAction(nameof(UpdateMyDetails));
        }

        //get by Id
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null || employee.EmpIsDeleted)
                return NotFound();

            return View(employee);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);

            if (employee == null)
            {
                return NotFound("Employee record not found.");
            }

            return View("GetById", employee);
        }

    }
}
