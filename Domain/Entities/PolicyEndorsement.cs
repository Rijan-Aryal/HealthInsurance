using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class PolicyEndorsement : BaseEntity
{
    [Required]
    public Guid PolicyId { get; set; }

    public Policy Policy { get; set; } = null!;

    [Required]
    public EndorsementType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal? OldValue { get; set; }

    public decimal? NewValue { get; set; }

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}

