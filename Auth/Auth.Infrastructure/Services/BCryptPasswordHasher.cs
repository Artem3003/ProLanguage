using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Custom password hasher using BCrypt algorithm.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string HashPassword(ApplicationUser user, string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(WorkFactor));
    }

    /// <inheritdoc />
    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        // Check if this is a BCrypt hash (starts with $2)
        if (hashedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            if (!isValid)
            {
                return PasswordVerificationResult.Failed;
            }

            // Check if rehash is needed (work factor changed)
            var hashInfo = BCrypt.Net.BCrypt.InterrogateHash(hashedPassword);
            return int.TryParse(hashInfo.WorkFactor, out var currentWorkFactor) && currentWorkFactor < WorkFactor
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }

        // Handle legacy ASP.NET Identity hashes for migration
        // This allows existing users with PBKDF2 hashes to still log in
        // Their passwords will be rehashed to BCrypt on next login
        var legacyHasher = new PasswordHasher<ApplicationUser>();
        var legacyResult = legacyHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return legacyResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Failed;
    }
}
