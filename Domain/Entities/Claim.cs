using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Claim : BaseEntity
{
    public Guid PolicyId { get; set; }
    
    public Policy? Policy { get; set; }
    
    public decimal ClaimAmount { get; set; }
    
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    
    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string? AssignedAdjuster { get; set; }
    
    public string? AssignedAdjusterId { get; set; }
    
    public decimal? ApprovedAmount { get; set; }
    
    public string? ReviewNotes { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public string? PaidBy { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public bool IsFlaggedForReview { get; set; }
    
    public string? FlagReason { get; set; }
    
    public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    
    public ICollection<ClaimAuditLog> AuditLogs { get; set; } = new List<ClaimAuditLog>();
}

