using Microsoft.Extensions.Options;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Services
{
    public class TestEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public TestEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Simulate email sending for development
            Console.WriteLine("=== TEST EMAIL SERVICE ===");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("=== EMAIL WOULD BE SENT ===");
            
            // Simulate async operation
            await Task.Delay(100);
            
            Console.WriteLine($"Test email 'sent' to: {to}");
        }
    }
}
