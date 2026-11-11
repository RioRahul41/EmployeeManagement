using CodexaManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.IRepositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task AddProject(Project project);
        Task DeleteAsync (Project project);
        Task UpdateAsync (Project project);
        Task SavechangeAsync();
    }
}
