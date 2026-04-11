using System.Net;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(IEmailService emailService, IConfiguration configuration, ILogger<ContactController> logger) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ContactController> _logger = logger;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Send([FromBody] ContactRequestDto request)
    {
        var recipient = _configuration["ContactSettings:RecipientEmail"] ?? "ayurchenko987@gmail.com";

        var safeName = WebUtility.HtmlEncode(request.Name.Trim());
        var safeSurname = WebUtility.HtmlEncode(request.Surname.Trim());
        var safeEmail = WebUtility.HtmlEncode(request.Email.Trim());
        var safeMessage = WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br />");

        var subject = $"Contact request from {request.Name} {request.Surname}";
        var body = $"""
            <h2>New Contact Request</h2>
            <p><strong>Name:</strong> {safeName}</p>
            <p><strong>Surname:</strong> {safeSurname}</p>
            <p><strong>Email:</strong> {safeEmail}</p>
            <p><strong>Message:</strong></p>
            <p>{safeMessage}</p>
            """;

        await _emailService.SendEmailAsync(recipient, subject, body);
        _logger.LogInformation("Contact request sent from {Email} to {Recipient}", request.Email, recipient);

        return Ok(new { message = "Message sent successfully." });
    }
}
