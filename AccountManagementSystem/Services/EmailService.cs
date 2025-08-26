using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                
                Console.WriteLine($"Email sent successfully to: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                // In production, you might want to log this error or handle it differently
                throw;
            }
        }
    }
}
