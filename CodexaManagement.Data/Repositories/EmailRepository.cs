using CodexaManagement.Core.IRepositories;
using CodexaManagement.Data.DbContext;
using CodexaManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Data.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(EmailLog emailLog)
        {
           await _context.EmailLogs.AddAsync(emailLog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmailLog emailLog)
        {

             _context.EmailLogs.Update(emailLog);
            await _context.SaveChangesAsync();
    
        }
    }
}
