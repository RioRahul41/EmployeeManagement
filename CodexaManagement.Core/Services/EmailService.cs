
using CodexaManagement.Core.Configuration;
using CodexaManagement.Core.IRepositories;
using CodexaManagement.Core.IServices;
using CodexaManagement.Model.Entities;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace CodexaManagement.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _settings;
        private readonly IEmailRepository _repository;

        public EmailService(
            IOptions<MailSettings> options,
            IEmailRepository emailLogRepository)
        {
            _settings = options.Value;
            _repository = emailLogRepository;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            string emailType = "General")
        {
            // Step 1: Save the email attempt before sending.
            var log = new EmailLog
            {
                ToEmail = toEmail,
                Subject = subject,
                EmailType = emailType,
                Status = "Pending",
                CreatedTime = DateTime.UtcNow,
                ErrorMessage = null
            };

            await _repository.AddAsync(log);

            try
            {
                // Step 2: Prepare the email.
                using var message = new MailMessage();

                message.From = new MailAddress(
                    _settings.FromEmail,
                    _settings.FromName);

                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                // Step 3: Send the email.
                using var smtp = new SmtpClient(
                    _settings.Host,
                    _settings.Port);

                await smtp.SendMailAsync(message);

                // Step 4: Record successful sending.
                log.Status = "Sent";
                log.SentTime = DateTime.UtcNow;
                log.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                // Step 5: Record the failure.
                log.Status = "Failed";
                log.ErrorMessage = ex.Message;

                throw;
            }
            finally
            {
                // Step 6: Persist the final status.
                await _repository.UpdateAsync(log);
            }
        }
    }
}
