using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ClaimAuditLog : BaseEntity
{
    public Guid ClaimId { get; set; }
    
    public Claim? Claim { get; set; }
    
    public ClaimStatus FromStatus { get; set; }
    
    public ClaimStatus ToStatus { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    public string? PerformedBy { get; set; }
    
    public string? Notes { get; set; }
}

