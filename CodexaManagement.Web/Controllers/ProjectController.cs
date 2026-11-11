using CodexaManagement.Core.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodexaManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _Service;
        public ProjectController(IProjectService Service)
        {
            _Service = Service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await _Service.GetAllAsync();

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _Service.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CodexaManagement.Model.Entities.Project
            {
                StartDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProjectName,Description,StartDate,EndDate")] CodexaManagement.Model.Entities.Project project)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            ModelState.Remove(nameof(CodexaManagement.Model.Entities.Project.CreatedByUserId));
            project.CreatedByUserId = userId;

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            await _Service.AddProject(project);

            TempData["Success"] = "Project created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _Service.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,ProjectName,Description,StartDate,EndDate,IsActive")] CodexaManagement.Model.Entities.Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CodexaManagement.Model.Entities.Project.CreatedByUserId));

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            var existingProject = await _Service.GetByIdAsync(id);

            if (existingProject == null)
            {
                return NotFound();
            }

            existingProject.ProjectName = project.ProjectName;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.IsActive = project.IsActive;

            await _Service.UpdateAsync(existingProject);

            TempData["Success"] = "Project updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _Service.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _Service.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            await _Service.DeleteAsync(id);

            TempData["Success"] = "Project deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
