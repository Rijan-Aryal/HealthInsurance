using Domain.Enums;

namespace Application.DTOs;

public class ClaimDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public decimal ClaimAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public string? Description { get; set; }
    public string? AssignedAdjuster { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaidBy { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsFlaggedForReview { get; set; }
    public string? FlagReason { get; set; }
    public List<ClaimDocumentDto> Documents { get; set; } = new();
    public List<ClaimAuditLogDto> AuditLogs { get; set; } = new();
}

public class CreateClaimDto
{
    public Guid PolicyId { get; set; }
    public decimal ClaimAmount { get; set; }
    public string? Description { get; set; }
}

public class UpdateClaimDto
{
    public decimal ClaimAmount { get; set; }
    public string? Description { get; set; }
}

public class ReviewClaimDto
{
    public string Action { get; set; } = string.Empty; // Approve or Reject
    public decimal? ApprovedAmount { get; set; }
    public string? ReviewNotes { get; set; }
    public string? AssignedAdjuster { get; set; }
    public string? PerformedBy { get; set; }
}

public class PayClaimDto
{
    public string? PaidBy { get; set; }
    public string? Notes { get; set; }
}

public class AssignAdjusterDto
{
    public string AdjusterName { get; set; } = string.Empty;
    public string? AdjusterId { get; set; }
    public string? Notes { get; set; }
}

