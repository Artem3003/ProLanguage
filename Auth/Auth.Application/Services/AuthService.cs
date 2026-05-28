using System.IdentityModel.Tokens.Jwt;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Data;
using Auth.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Application.Services;

/// <summary>
/// Service for authentication operations.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="signInManager">The sign-in manager.</param>
/// <param name="tokenService">The token service.</param>
/// <param name="context">The database context.</param>
/// <param name="configuration">The configuration.</param>
/// <param name="emailService">The email service.</param>
/// <param name="logger">The logger.</param>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    AuthDbContext context,
    IConfiguration configuration,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress)
    {
        logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration failed: email {Email} already exists", request.Email);
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "A user with this email already exists.",
            };
        }

        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Passwords do not match.",
            };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = errors,
            };
        }

        // Assign default role
        await userManager.AddToRoleAsync(user, "Student");

        logger.LogInformation("User {Email} registered successfully", request.Email);

        // Generate tokens
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiration) = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken(ipAddress);

        // Add refresh token directly to context with UserId set
        refreshToken.UserId = user.Id;
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return new AuthResponseDto
        {
            IsSuccess = true,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = expiration,
            Roles = roles,
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress)
    {
        logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Login failed: user {Email} not found", request.Email);
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email or password.",
            };
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login failed: user {Email} is deactivated", request.Email);
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Your account has been deactivated. Please contact support.",
            };
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                logger.LogWarning("Login failed: user {Email} is locked out", request.Email);
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Your account has been locked due to multiple failed login attempts. Please try again later.",
                };
            }

            logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email or password.",
            };
        }

        // Generate tokens
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiration) = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken(ipAddress);

        // Remove old expired refresh tokens
        await RemoveOldRefreshTokensAsync(user.Id);

        // Add refresh token directly to context
        refreshToken.UserId = user.Id;
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        logger.LogInformation("User {Email} logged in successfully", request.Email);

        return new AuthResponseDto
        {
            IsSuccess = true,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = expiration,
            Roles = roles,
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress)
    {
        var userId = tokenService.GetUserIdFromToken(request.AccessToken);
        if (userId == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid access token.",
            };
        }

        var user = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "User not found.",
            };
        }

        var refreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == request.RefreshToken);
        if (refreshToken == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid refresh token.",
            };
        }

        if (!refreshToken.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Refresh token is expired or revoked.",
            };
        }

        // Revoke old refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = "Replaced by new token";

        // Generate new tokens
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiration) = tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = tokenService.GenerateRefreshToken(ipAddress);

        refreshToken.ReplacedByToken = newRefreshToken.Token;

        // Add new refresh token directly to context
        newRefreshToken.UserId = user.Id;
        context.RefreshTokens.Add(newRefreshToken);

        await context.SaveChangesAsync();

        logger.LogInformation("Token refreshed for user {UserId}", user.Id);

        return new AuthResponseDto
        {
            IsSuccess = true,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiration = expiration,
            Roles = roles,
        };
    }

    /// <inheritdoc />
    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress)
    {
        var token = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = "Revoked by user";

        await context.SaveChangesAsync();

        logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> LogoutAsync(Guid userId, string? ipAddress)
    {
        var user = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Logged out";
        }

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} logged out", userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> ExternalLoginAsync(ExternalLoginDto request, string? ipAddress)
    {
        logger.LogInformation("External login attempt with provider: {Provider}", request.Provider);

        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.IdToken))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Provider and ID token are required.",
            };
        }

        // Validate token based on provider
        ExternalUserInfo? userInfo;
        if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            userInfo = await ValidateGoogleTokenAsync(request.IdToken);
        }
        else if (request.Provider.Equals("Apple", StringComparison.OrdinalIgnoreCase))
        {
            userInfo = await ValidateAppleTokenAsync(request.IdToken);
        }
        else
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Unsupported authentication provider.",
            };
        }

        if (userInfo == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = $"Invalid {request.Provider} token.",
            };
        }

        // For Apple, use the name from request if provided (only sent on first authorization)
        if (request.Provider.Equals("Apple", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                userInfo.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                userInfo.LastName = request.LastName;
            }
        }

        // Check if user exists
        var user = await userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            // Create new user from external account
            user = new ApplicationUser
            {
                UserName = userInfo.Email,
                Email = userInfo.Email,
                EmailConfirmed = userInfo.EmailVerified,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("External login user creation failed for {Email}: {Errors}", userInfo.Email, errors);
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = errors,
                };
            }

            // Add external login info
            var loginInfo = new UserLoginInfo(request.Provider, userInfo.ProviderKey, request.Provider);
            await userManager.AddLoginAsync(user, loginInfo);

            // Assign default role
            await userManager.AddToRoleAsync(user, "Student");

            logger.LogInformation("New user {Email} created via {Provider} login", userInfo.Email, request.Provider);
        }
        else
        {
            // Check if user has this provider login linked
            var logins = await userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == request.Provider && l.ProviderKey == userInfo.ProviderKey))
            {
                // Link external account to existing user
                var loginInfo = new UserLoginInfo(request.Provider, userInfo.ProviderKey, request.Provider);
                await userManager.AddLoginAsync(user, loginInfo);
                logger.LogInformation("{Provider} login linked to existing user {Email}", request.Provider, userInfo.Email);
            }

            if (!user.IsActive)
            {
                logger.LogWarning("External login failed: user {Email} is deactivated", userInfo.Email);
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Your account has been deactivated. Please contact support.",
                };
            }
        }

        // Generate tokens
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiration) = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken(ipAddress);

        // Remove old expired refresh tokens
        await RemoveOldRefreshTokensAsync(user.Id);

        // Add refresh token directly to context
        refreshToken.UserId = user.Id;
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        logger.LogInformation("User {Email} logged in via {Provider} successfully", user.Email, request.Provider);

        return new AuthResponseDto
        {
            IsSuccess = true,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = expiration,
            Roles = roles,
        };
    }

    /// <inheritdoc />
    public async Task<PasswordResetResponseDto> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        logger.LogInformation("Password reset requested for email: {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "User with this email does not exist.",
            };
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password reset requested for deactivated account: {Email}", request.Email);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "This account has been deactivated.",
            };
        }

        // Generate password reset token using ASP.NET Core Identity
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        logger.LogInformation("Password reset token generated for user: {Email}", request.Email);

        // Send password reset email
        try
        {
            await emailService.SendPasswordResetEmailAsync(request.Email, token);
            logger.LogInformation("Password reset email sent to: {Email}", request.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to: {Email}", request.Email);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Failed to send password reset email. Please try again later.",
            };
        }

        return new PasswordResetResponseDto
        {
            IsSuccess = true,
            Message = "A password reset link has been sent to your email.",
        };
    }

    /// <inheritdoc />
    public async Task<PasswordResetResponseDto> ResetPasswordAsync(ResetPasswordDto request)
    {
        logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Password reset failed: user {Email} not found", request.Email);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid request.",
            };
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password reset failed: user {Email} is deactivated", request.Email);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Your account has been deactivated. Please contact support.",
            };
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Passwords do not match.",
            };
        }

        // Reset the password using ASP.NET Core Identity
        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Password reset failed for {Email}: {Errors}", request.Email, errors);
            return new PasswordResetResponseDto
            {
                IsSuccess = false,
                ErrorMessage = errors.Contains("Invalid token") ? "The password reset link has expired or is invalid." : errors,
            };
        }

        logger.LogInformation("Password reset successful for user: {Email}", request.Email);

        // Revoke all refresh tokens for security
        await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
                .SetProperty(rt => rt.RevokedByIp, "PasswordReset"));

        return new PasswordResetResponseDto
        {
            IsSuccess = true,
            Message = "Your password has been reset successfully. You can now log in with your new password.",
        };
    }

    private async Task<ExternalUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var googleClientId = configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                logger.LogError("Google ClientId is not configured");
                return null;
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return payload == null || string.IsNullOrEmpty(payload.Email)
                ? null
                : new ExternalUserInfo
                {
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "User",
                    LastName = payload.FamilyName ?? payload.Name?.Split(' ').LastOrDefault() ?? string.Empty,
                    ProviderKey = payload.Subject,
                };
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning("Invalid Google token: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<ExternalUserInfo?> ValidateAppleTokenAsync(string idToken)
    {
        try
        {
            var appleClientId = configuration["AppleAuth:ClientId"];
            if (string.IsNullOrEmpty(appleClientId))
            {
                logger.LogError("Apple ClientId is not configured");
                return null;
            }

            // Get Apple's public keys
            using var httpClient = new HttpClient();
            var keysResponse = await httpClient.GetStringAsync("https://appleid.apple.com/auth/keys");
            var jwks = new JsonWebKeySet(keysResponse);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = appleClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
            };

            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return null;
            }

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var emailVerified = jwtToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true";
            var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
            {
                logger.LogWarning("Apple token missing required claims");
                return null;
            }

            // Apple doesn't provide name in subsequent logins, only on first authorization
            // The name is passed separately in the authorization response
            return new ExternalUserInfo
            {
                Email = email,
                EmailVerified = emailVerified,
                FirstName = "User",
                LastName = string.Empty,
                ProviderKey = sub,
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning("Invalid Apple token: {Message}", ex.Message);
            return null;
        }
    }

    private async Task RemoveOldRefreshTokensAsync(Guid userId)
    {
        await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
