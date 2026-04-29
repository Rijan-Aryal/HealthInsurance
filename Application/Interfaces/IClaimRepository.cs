using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IClaimRepository
{
    Task<List<Claim>> GetAllAsync();
    Task<List<Claim>> GetByPolicyIdAsync(Guid policyId);
    Task<List<Claim>> GetByStatusAsync(ClaimStatus status);
    Task<Claim?> GetByIdAsync(Guid id);
    Task<Claim> AddAsync(Claim claim);
    Task UpdateAsync(Claim claim);
    Task DeleteAsync(Guid id);
    
    // Workflow methods
    Task AddAuditLogAsync(ClaimAuditLog auditLog);
    Task<List<ClaimAuditLog>> GetAuditLogsAsync(Guid claimId);
    Task AddDocumentAsync(ClaimDocument document);
    Task<List<ClaimDocument>> GetDocumentsAsync(Guid claimId);
    Task<bool> CanTransitionAsync(ClaimStatus fromStatus, ClaimStatus toStatus);
}

