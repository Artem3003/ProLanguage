// <copyright file="EmailSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Auth.Infrastructure.Settings;

/// <summary>
/// Settings for email configuration.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Gets or sets the SMTP server host.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username (email address).
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password (app password for Gmail).
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string SenderName { get; set; } = "ProLanguage";

    /// <summary>
    /// Gets or sets the frontend URL for password reset links.
    /// </summary>
    public string FrontendUrl { get; set; } = "http://localhost:4200";

    /// <summary>
    /// Gets or sets a value indicating whether SSL/TLS is enabled.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
}
