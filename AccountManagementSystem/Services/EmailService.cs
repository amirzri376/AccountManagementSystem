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
                // Debug: Print configuration (remove in production)
                Console.WriteLine($"SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"Username: {_emailSettings.SmtpUsername}");
                Console.WriteLine($"Password Length: {_emailSettings.SmtpPassword?.Length ?? 0}");
                Console.WriteLine($"From Email: {_emailSettings.FromEmail}");

                // Try different Gmail configurations
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds timeout
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
                Console.WriteLine($"SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"Username: {_emailSettings.SmtpUsername}");
                Console.WriteLine($"Password Length: {_emailSettings.SmtpPassword?.Length ?? 0}");
                
                // Try alternative configuration if first fails
                try
                {
                    Console.WriteLine("Trying alternative Gmail configuration...");
                    using var client2 = new SmtpClient("smtp.gmail.com", 465)
                    {
                        EnableSsl = true,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Timeout = 30000
                    };

                    var message2 = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };
                    message2.To.Add(to);

                    await client2.SendMailAsync(message2);
                    Console.WriteLine($"Email sent successfully with alternative config to: {to}");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Alternative config also failed: {ex2.Message}");
                    throw ex; // Throw the original exception
                }
            }
        }
    }
}
