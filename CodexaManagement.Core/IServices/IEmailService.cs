using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            string emailType = "General");
    }
}

