using Domain.Enums;

namespace Application.DTOs;

public class ClaimAuditLogDto
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}

