using CodexaManagement.Core.IRepositories;
using CodexaManagement.Core.IServices;
using CodexaManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }

        public async Task AddProject(Project project)
        {
            project.CreatedTime = DateTime.Now;
            project.IsActive=true;
            project.IsDeleted=false;
            await _repository.AddProject(project);
            await _repository.SavechangeAsync();
          
        }

        public async Task DeleteAsync(int Id)
        {
            var project = await _repository.GetByIdAsync(Id);
            if (project != null)
            {
                await _repository.DeleteAsync(project);
                await _repository.SavechangeAsync();
            }
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
          return await _repository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Project project)
        {
           var exisiting = await _repository.GetByIdAsync(project.Id);
            if (exisiting == null)
            {
                throw new Exception("Project Not Found");
            }
            exisiting.ProjectName = project.ProjectName;
            exisiting.Description = project.Description;
           exisiting.StartDate = project.StartDate;
            exisiting.EndDate = project.EndDate;
            exisiting.IsActive = project.IsActive;
            exisiting.UpdatedTime = DateTime.Now;
            await _repository.UpdateAsync(exisiting);
            await _repository.SavechangeAsync();
        }
    }
}
