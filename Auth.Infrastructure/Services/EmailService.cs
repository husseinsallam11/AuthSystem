using Auth.Application.Configuration;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Auth.Infrastructure.Services
{
    public class EmailService : IEmailSender<ApplicationUser>
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationlink)
        {
            const string subject = "Confirm your email";
            var body = $$"""
            <h1>Welcome, {{user.UserName}}!</h1>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{{confirmationlink}}">Confirm my email</a></p>
            <p>This link expires in {{_settings.ConfirmationTokenLifetimeHours}} hours.</p>
            <p>If you didn't sign up, you can ignore this email.</p>
        """;
            return SendAsync(email, subject, body);
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            const string subject = "Reset your password";
            var body = $$"""
            <h1>Hi {{user.UserName}},</h1>
            <p>Click the link below to reset your password:</p>
            <p><a href="{{resetLink}}">Reset my password</a></p>
            <p>If you didn't request this, you can ignore this email.</p>
        """;
            return SendAsync(email, subject, body);
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            const string subject = "Reset your password";
            var body = $$"""
            <h1>Hi {{user.UserName}},</h1>
            <p>Use the code below to reset your password:</p>
            <h2>{{resetCode}}</h2>
            <p>If you didn't request this, you can ignore this email.</p>
        """;
            return SendAsync(email, subject, body);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        => SendAsync(email, subject, htmlMessage);
        private async Task SendAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    _settings.UseStartTls ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto);

                if (!string.IsNullOrEmpty(_settings.SmtpUsername))
                {
                    await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                await client.SendAsync(message);
                await client.DisconnectAsync(quit: true);

                _logger.LogInformation("Email sent to {Email} with subject {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }
    }
}
    
