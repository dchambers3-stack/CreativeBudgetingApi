using Microsoft.Extensions.Logging;

namespace CreativeBudgeting.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // Simulate email sending with a small delay
            await Task.Delay(500);

            // Log the email details instead of actually sending
            _logger.LogInformation("=== MOCK EMAIL SENT ===");
            _logger.LogInformation("To: {ToEmail}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Content: {Content}", htmlContent);
            _logger.LogInformation("========================");

            // For development, we'll just pretend it was sent successfully
            Console.WriteLine($"?? Mock email sent to: {toEmail}");
            Console.WriteLine($"?? Subject: {subject}");
            Console.WriteLine($"?? Content: {htmlContent}");
        }
    }
}