using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Domain.Data;

/// <summary>
/// Database context for the Payments domain.
/// </summary>
/// <param name="options">The database context options.</param>
public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the payments collection.
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Gets or sets the transactions collection.
    /// </summary>
    public DbSet<Transaction> Transactions { get; set; }

    /// <summary>
    /// Configures the entity models.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.Currency)
                .HasMaxLength(3);
            
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.FailureReason)
                .HasMaxLength(1000);

            entity.HasMany(e => e.Transactions)
                .WithOne(t => t.Payment)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50);
            
            entity.Property(e => e.Status)
                .HasMaxLength(50);
            
            entity.Property(e => e.GatewayTransactionId)
                .HasMaxLength(100);

            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
