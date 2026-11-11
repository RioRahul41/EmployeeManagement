using CodexaManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace CodexaManagement.Core.IServices
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task AddProject(Project project);
        Task DeleteAsync(int Id);
        Task UpdateAsync(Project project);

    }
}
