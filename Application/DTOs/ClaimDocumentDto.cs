namespace Application.DTOs;

public class ClaimDocumentDto
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateClaimDocumentDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

