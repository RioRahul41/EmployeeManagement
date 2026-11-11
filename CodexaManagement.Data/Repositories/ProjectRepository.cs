using System;
using System.Collections.Generic;
using System.Text;
using CodexaManagement.Core.IRepositories;
using CodexaManagement.Data.DbContext;
using CodexaManagement.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodexaManagement.Data.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProject(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public async Task DeleteAsync(Project project)
        {
            project.IsDeleted = true;
            project.IsActive = false;
            project.UpdatedTime= DateTime.Now;
            _context.Projects.Update(project);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            var project = await _context.Projects
                .Where(e => !e.IsDeleted && e.IsActive)
                .OrderByDescending(e => e.CreatedTime)
                .ToListAsync();
            return project;
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            return project;
        }

        public async Task SavechangeAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            return Task.CompletedTask;
        }
    }
}
