using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Payments.Domain.Data;

namespace Payments.Migrations;

public class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<PaymentsDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PaymentsConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:PaymentsConnection is not set. Provide it via " +
                "env var ConnectionStrings__PaymentsConnection or " +
                "`dotnet user-secrets set \"ConnectionStrings:PaymentsConnection\" \"<value>\" --project Payments/Payments.Migrations`.");

        var optionsBuilder = new DbContextOptionsBuilder<PaymentsDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("Payments.Migrations"));

        return new PaymentsDbContext(optionsBuilder.Options);
    }
}
