using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CreativeBudgeting.Services
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // Validate configuration
            var fromEmail = _configuration["Email:FromEmail"];
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration["Email:SmtpPort"];
            var password = _configuration["Email:Password"];

            if (string.IsNullOrEmpty(fromEmail))
                throw new InvalidOperationException("Email:FromEmail configuration is missing");
            if (string.IsNullOrEmpty(smtpHost))
                throw new InvalidOperationException("Email:SmtpHost configuration is missing");
            if (string.IsNullOrEmpty(smtpPort))
                throw new InvalidOperationException("Email:SmtpPort configuration is missing");
            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Email:Password configuration is missing");
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentException("Recipient email address cannot be null or empty", nameof(toEmail));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Support Team", fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlContent
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                smtpHost,
                int.Parse(smtpPort),
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                fromEmail,
                password
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
