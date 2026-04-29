using Application.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IPolicyRepository
{
    Task<List<PolicyDto>> GetAllAsync();
    Task<PolicyDto?> GetByIdAsync(Guid id);
    Task<Policy> AddAsync(Policy policy);
    Task<Policy?> UpdateAsync(Guid id, Policy policy);
    Task<bool> DeleteAsync(Guid id);

    // Lifecycle operations
    Task<Policy?> UpdateStatusAsync(Guid id, PolicyStatus status);
    Task<Policy?> ActivateAsync(Guid id);
    Task<Policy?> CancelAsync(Guid id);
    Task<Policy?> ExpireAsync(Guid id);
    Task<Policy?> RenewAsync(Guid id, DateTime newEndDate);

    // Endorsements
    Task<PolicyEndorsement> AddEndorsementAsync(Guid policyId, PolicyEndorsement endorsement);
    Task<List<EndorsementDto>> GetEndorsementsAsync(Guid policyId);

    // Queries
    Task<List<PolicyDto>> GetByStatusAsync(PolicyStatus status);
    Task<List<PolicyDto>> GetExpiredAsync();
    Task<List<PolicyDto>> GetUpForRenewalAsync(int daysBeforeExpiry = 7);
    Task<List<PolicyDto>> GetPendingAutoRenewalAsync();
}

