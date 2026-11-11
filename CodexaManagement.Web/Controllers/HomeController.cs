using CodexaManagement.Core.IServices;
using CodexaManagement.Core.Services;
using CodexaManagement.Model.Entities;
using CodexaManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CodexaManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,HR,User,Manager")]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityApplication> _userManager;
        private readonly IEmployeService _employee;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(UserManager<IdentityApplication> userManager,
             RoleManager<IdentityRole> roleManager,
            IEmployeService employee)
        {
            _userManager = userManager;
            _employee = employee;
            _roleManager = roleManager;
        }
        

        public async Task<IActionResult> Index()
        {
            var user =  _userManager.Users.Count();
            ViewBag.User = user;
            var emp = await _employee.GetAllEmployeesAsync();

            ViewBag.TotalEmployees = emp.Count();
            ViewBag.RoleCount = await _roleManager.Roles.CountAsync();

            var employees = (await _employee.GetAllEmployeesAsync()).ToList();

            ViewBag.TotalEmployees = employees.Count;
            ViewBag.ActiveEmployees = employees.Count(e => e.EmpIsActive);
            ViewBag.InactiveEmployees = employees.Count(e => !e.EmpIsActive);

            return View();
        }

       
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
