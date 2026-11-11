using CodexaManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.IRepositories
{
    public interface IEmailRepository
    {
        Task AddAsync(EmailLog emailLog);
        Task UpdateAsync(EmailLog emailLog);
    }
}
