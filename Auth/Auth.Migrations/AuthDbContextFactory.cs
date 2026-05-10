using Auth.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Auth.Migrations;

/// <summary>
/// Factory for creating instances of <see cref="AuthDbContext"/> at design time.
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="AuthDbContext"/> using the specified arguments.
    /// </summary>
    /// <param name="args">Arguments for creating the DbContext.</param>
    /// <returns>A new instance of <see cref="AuthDbContext"/>.</returns>
    public AuthDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Auth.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = configuration.GetConnectionString("AuthConnection");

        optionsBuilder.UseSqlServer(connectionString, b =>
            b.MigrationsAssembly("Auth.Migrations"));

        return new AuthDbContext(optionsBuilder.Options);
    }
}