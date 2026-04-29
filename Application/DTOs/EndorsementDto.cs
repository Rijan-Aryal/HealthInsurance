using Domain.Enums;

namespace Application.DTOs;

public class EndorsementDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public EndorsementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? OldValue { get; set; }
    public decimal? NewValue { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateEndorsementDto
{
    public EndorsementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? OldValue { get; set; }
    public decimal? NewValue { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}

