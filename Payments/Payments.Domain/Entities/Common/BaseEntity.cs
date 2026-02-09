using System.ComponentModel.DataAnnotations;

namespace Payments.Domain.Entities.Common;

/// <summary>
/// Base entity class with a generic identifier type.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class BaseEntity<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [Key]
    public TId? Id { get; set; }
}
