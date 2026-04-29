using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimController : ControllerBase
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IWebHostEnvironment _env;

    public ClaimController(IClaimRepository claimRepository, IPolicyRepository policyRepository, IWebHostEnvironment env)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _env = env;
    }

    private ClaimDto MapToDto(Claim claim)
    {
        return new ClaimDto
        {
            Id = claim.Id,
            PolicyId = claim.PolicyId,
            ClaimAmount = claim.ClaimAmount,
            Status = claim.Status.ToString(),
            ClaimDate = claim.ClaimDate,
            Description = claim.Description,
            AssignedAdjuster = claim.AssignedAdjuster,
            ApprovedAmount = claim.ApprovedAmount,
            ReviewNotes = claim.ReviewNotes,
            ReviewedAt = claim.ReviewedAt,
            PaidAt = claim.PaidAt,
            PaidBy = claim.PaidBy,
            RejectionReason = claim.RejectionReason,
            IsFlaggedForReview = claim.IsFlaggedForReview,
            FlagReason = claim.FlagReason,
            Documents = claim.Documents?.Select(d => new ClaimDocumentDto
            {
                Id = d.Id,
                ClaimId = d.ClaimId,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                DocumentType = d.DocumentType,
                Description = d.Description,
                CreatedDate = d.CreatedDate
            }).ToList() ?? new List<ClaimDocumentDto>(),
            AuditLogs = claim.AuditLogs?.Select(a => new ClaimAuditLogDto
            {
                Id = a.Id,
                ClaimId = a.ClaimId,
                FromStatus = a.FromStatus.ToString(),
                ToStatus = a.ToStatus.ToString(),
                Action = a.Action,
                PerformedBy = a.PerformedBy,
                Notes = a.Notes,
                CreatedDate = a.CreatedDate
            }).ToList() ?? new List<ClaimAuditLogDto>()
        };
    }

    [HttpGet]
    public async Task<ActionResult<List<ClaimDto>>> GetAll()
    {
        var claims = await _claimRepository.GetAllAsync();
        return Ok(claims.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimDto>> GetById(Guid id)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });
        return Ok(MapToDto(claim));
    }

    [HttpGet("policy/{policyId}")]
    public async Task<ActionResult<List<ClaimDto>>> GetByPolicy(Guid policyId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId);
        if (policy == null) return NotFound(new { message = "Policy not found" });
        var claims = await _claimRepository.GetByPolicyIdAsync(policyId);
        return Ok(claims.Select(MapToDto));
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<ClaimDto>>> GetByStatus(ClaimStatus status)
    {
        var claims = await _claimRepository.GetByStatusAsync(status);
        return Ok(claims.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<ClaimDto>> Create(CreateClaimDto dto)
    {
        var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
        if (policy == null) return NotFound(new { message = "Policy not found" });

        var claim = new Claim
        {
            PolicyId = dto.PolicyId,
            ClaimAmount = dto.ClaimAmount,
            Description = dto.Description,
            Status = ClaimStatus.Submitted
        };

        var created = await _claimRepository.AddAsync(claim);

        // Add initial audit log
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = created.Id,
            FromStatus = ClaimStatus.Submitted,
            ToStatus = ClaimStatus.Submitted,
            Action = "Claim Submitted",
            PerformedBy = "Customer",
            Notes = $"New claim submitted for amount {dto.ClaimAmount:C}"
        });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClaimDto>> Update(Guid id, UpdateClaimDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (claim.Status != ClaimStatus.Submitted)
            return BadRequest(new { message = "Can only update claims in Submitted status" });

        claim.ClaimAmount = dto.ClaimAmount;
        claim.Description = dto.Description;

        await _claimRepository.UpdateAsync(claim);
        return Ok(MapToDto(claim));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (claim.Status != ClaimStatus.Submitted && claim.Status != ClaimStatus.Rejected)
            return BadRequest(new { message = "Can only delete claims in Submitted or Rejected status" });

        await _claimRepository.DeleteAsync(id);
        return NoContent();
    }

    // === WORKFLOW ENDPOINTS ===

    [HttpPost("{id}/submit-to-review")]
    public async Task<ActionResult<ClaimDto>> SubmitToReview(Guid id)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (claim.Status != ClaimStatus.Submitted)
            return BadRequest(new { message = "Only Submitted claims can be moved to UnderReview" });

        var canTransition = await _claimRepository.CanTransitionAsync(claim.Status, ClaimStatus.UnderReview);
        if (!canTransition)
            return BadRequest(new { message = "Invalid status transition" });

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.UnderReview;
        claim.ReviewedAt = DateTime.UtcNow;

        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = claim.Id,
            FromStatus = oldStatus,
            ToStatus = ClaimStatus.UnderReview,
            Action = "Moved to UnderReview",
            PerformedBy = "System/Adjuster",
            Notes = "Claim submitted for review"
        });

        return Ok(MapToDto(claim));
    }

    [HttpPost("{id}/review")]
    public async Task<ActionResult<ClaimDto>> Review(Guid id, ReviewClaimDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (claim.Status != ClaimStatus.UnderReview)
            return BadRequest(new { message = "Only UnderReview claims can be reviewed" });

        var oldStatus = claim.Status;

        if (dto.Action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
        {
            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = dto.ApprovedAmount ?? claim.ClaimAmount;
            claim.ReviewNotes = dto.ReviewNotes;
            claim.AssignedAdjuster = dto.AssignedAdjuster ?? claim.AssignedAdjuster;
        }
        else if (dto.Action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = dto.ReviewNotes ?? "No reason provided";
            claim.AssignedAdjuster = dto.AssignedAdjuster ?? claim.AssignedAdjuster;
        }
        else
        {
            return BadRequest(new { message = "Action must be 'Approve' or 'Reject'" });
        }

        claim.ReviewedAt = DateTime.UtcNow;
        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = claim.Id,
            FromStatus = oldStatus,
            ToStatus = claim.Status,
            Action = $"Claim {dto.Action}d",
            PerformedBy = dto.PerformedBy ?? "Adjuster",
            Notes = dto.ReviewNotes
        });

        return Ok(MapToDto(claim));
    }

    [HttpPost("{id}/pay")]
    public async Task<ActionResult<ClaimDto>> Pay(Guid id, PayClaimDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (claim.Status != ClaimStatus.Approved)
            return BadRequest(new { message = "Only Approved claims can be paid" });

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Paid;
        claim.PaidAt = DateTime.UtcNow;
        claim.PaidBy = dto.PaidBy ?? "System";

        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = claim.Id,
            FromStatus = oldStatus,
            ToStatus = ClaimStatus.Paid,
            Action = "Claim Paid",
            PerformedBy = dto.PaidBy ?? "System",
            Notes = $"Amount paid: {claim.ApprovedAmount ?? claim.ClaimAmount:C}. {dto.Notes}"
        });

        return Ok(MapToDto(claim));
    }

    [HttpPost("{id}/assign-adjuster")]
    public async Task<ActionResult<ClaimDto>> AssignAdjuster(Guid id, AssignAdjusterDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        claim.AssignedAdjuster = dto.AdjusterName;
        claim.AssignedAdjusterId = dto.AdjusterId;

        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = claim.Id,
            FromStatus = claim.Status,
            ToStatus = claim.Status,
            Action = "Adjuster Assigned",
            PerformedBy = "Manager",
            Notes = $"Assigned to {dto.AdjusterName}. {dto.Notes}"
        });

        return Ok(MapToDto(claim));
    }

    [HttpPost("{id}/flag")]
    public async Task<ActionResult<ClaimDto>> FlagClaim(Guid id, [FromBody] string reason)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        claim.IsFlaggedForReview = true;
        claim.FlagReason = reason;

        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.AddAuditLogAsync(new ClaimAuditLog
        {
            ClaimId = claim.Id,
            FromStatus = claim.Status,
            ToStatus = claim.Status,
            Action = "Claim Flagged",
            PerformedBy = "System/Manager",
            Notes = reason
        });

        return Ok(MapToDto(claim));
    }

    // === DOCUMENT ENDPOINTS ===

    [HttpPost("{id}/documents")]
    public async Task<ActionResult<ClaimDocumentDto>> UploadDocument(Guid id, IFormFile file, [FromForm] string documentType, [FromForm] string? description)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "claims");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new ClaimDocument
        {
            ClaimId = id,
            FileName = file.FileName,
            FilePath = $"/uploads/claims/{fileName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            DocumentType = documentType,
            Description = description
        };

        await _claimRepository.AddDocumentAsync(document);

        return Ok(new ClaimDocumentDto
        {
            Id = document.Id,
            ClaimId = document.ClaimId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DocumentType = document.DocumentType,
            Description = document.Description,
            CreatedDate = document.CreatedDate
        });
    }

    [HttpGet("{id}/documents")]
    public async Task<ActionResult<List<ClaimDocumentDto>>> GetDocuments(Guid id)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        var documents = await _claimRepository.GetDocumentsAsync(id);
        return Ok(documents.Select(d => new ClaimDocumentDto
        {
            Id = d.Id,
            ClaimId = d.ClaimId,
            FileName = d.FileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            DocumentType = d.DocumentType,
            Description = d.Description,
            CreatedDate = d.CreatedDate
        }));
    }

    // === AUDIT LOG ENDPOINTS ===

    [HttpGet("{id}/audit-logs")]
    public async Task<ActionResult<List<ClaimAuditLogDto>>> GetAuditLogs(Guid id)
    {
        var claim = await _claimRepository.GetByIdAsync(id);
        if (claim == null) return NotFound(new { message = "Claim not found" });

        var logs = await _claimRepository.GetAuditLogsAsync(id);
        return Ok(logs.Select(a => new ClaimAuditLogDto
        {
            Id = a.Id,
            ClaimId = a.ClaimId,
            FromStatus = a.FromStatus.ToString(),
            ToStatus = a.ToStatus.ToString(),
            Action = a.Action,
            PerformedBy = a.PerformedBy,
            Notes = a.Notes,
            CreatedDate = a.CreatedDate
        }));
    }
}

