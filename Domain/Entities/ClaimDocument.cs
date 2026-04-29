using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ClaimDocument : BaseEntity
{
    public Guid ClaimId { get; set; }
    
    public Claim? Claim { get; set; }
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public string DocumentType { get; set; } = string.Empty; // Proof, Bill, MedicalReport, PoliceReport, Other
    
    public string? Description { get; set; }
}

