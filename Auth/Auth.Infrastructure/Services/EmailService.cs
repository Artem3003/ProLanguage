// <copyright file="EmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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

        var subject = "Password Reset Request - English School Platform";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>English School Platform</h1>
    </div>
    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #ddd; border-top: none;'>
        <h2 style='color: #333;'>Password Reset Request</h2>
        <p>Hello,</p>
        <p>We received a request to reset your password for your English School Platform account.</p>
        <p>Click the button below to reset your password:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Reset Password</a>
        </div>
        <p>Or copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #eee; padding: 10px; border-radius: 5px; font-size: 12px;'>{resetLink}</p>
        <p><strong>This link will expire in 24 hours.</strong></p>
        <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message from English School Platform. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
