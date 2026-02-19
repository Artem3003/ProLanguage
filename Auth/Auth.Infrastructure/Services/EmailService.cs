// <copyright file="EmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Reflection;
using System.Web;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Service for sending emails using SMTP.
/// </summary>
/// <param name="emailSettings">The email settings.</param>
/// <param name="logger">The logger.</param>
public class EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    /// <inheritdoc />
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _emailSettings.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : (_emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await client.ConnectAsync(
                _emailSettings.SmtpHost,
                _emailSettings.SmtpPort,
                secureSocketOptions);

            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var encodedToken = HttpUtility.UrlEncode(resetToken);
        var encodedEmail = HttpUtility.UrlEncode(toEmail);
        var resetLink = $"{_emailSettings.FrontendUrl}/reset-password?token={encodedToken}&email={encodedEmail}";

        var subject = "Password Reset Request - ProLanguage";
        var body = await LoadEmailTemplateAsync("PasswordResetEmail.html");
        body = body.Replace("{{RESET_LINK}}", resetLink);

        await SendEmailAsync(toEmail, subject, body);
    }

    private static async Task<string> LoadEmailTemplateAsync(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Auth.Infrastructure.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Email template '{templateName}' not found.");
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }
}
