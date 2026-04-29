using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _context;

    public PolicyRepository(AppDbContext context)
    {
        _context = context;
    }

    private static PolicyDto MapToDto(Policy p)
    {
        return new PolicyDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            CustomerId = p.CustomerId,
            Customer = new CustomerDto
            {
                Id = p.Customer.Id,
                FullName = p.Customer.FullName,
                Email = p.Customer.Email
            },
            PolicyType = p.PolicyType,
            Status = p.Status,
            CoverageAmount = p.CoverageAmount,
            Premium = p.Premium,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsAutoRenewal = p.IsAutoRenewal,
            RenewalReminderSent = p.RenewalReminderSent,
            OriginalPolicyId = p.OriginalPolicyId,
            Version = p.Version,
            CreatedDate = p.CreatedDate,
            Endorsements = p.Endorsements.Select(e => new EndorsementDto
            {
                Id = e.Id,
                PolicyId = e.PolicyId,
                Type = e.Type,
                Description = e.Description,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                EffectiveDate = e.EffectiveDate,
                CreatedDate = e.CreatedDate
            }).ToList()
        };
    }

    public async Task<List<PolicyDto>> GetAllAsync()
    {
        return await _context.Policies
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Endorsements)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PolicyDto?> GetByIdAsync(Guid id)
    {
        var policy = await _context.Policies
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.Id == id);

        return policy == null ? null : MapToDto(policy);
    }

    public async Task<Policy> AddAsync(Policy policy)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> UpdateAsync(Guid id, Policy policy)
    {
        var existing = await _context.Policies.FindAsync(id);
        if (existing == null) return null;

        existing.PolicyNumber = policy.PolicyNumber;
        existing.PolicyType = policy.PolicyType;
        existing.CoverageAmount = policy.CoverageAmount;
        existing.Premium = policy.Premium;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return false;

        _context.Policies.Remove(policy);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Policy?> UpdateStatusAsync(Guid id, PolicyStatus status)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return null;

        policy.Status = status;
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> ActivateAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return null;
        if (policy.Status != PolicyStatus.Draft)
            throw new InvalidOperationException("Only Draft policies can be activated.");

        policy.Status = PolicyStatus.Active;
        if (policy.StartDate == null)
            policy.StartDate = DateTime.UtcNow;
        if (policy.EndDate == null)
            policy.EndDate = DateTime.UtcNow.AddYears(1);

        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> CancelAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return null;
        if (policy.Status == PolicyStatus.Cancelled)
            throw new InvalidOperationException("Policy is already cancelled.");

        policy.Status = PolicyStatus.Cancelled;
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> ExpireAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return null;
        if (policy.Status != PolicyStatus.Active)
            throw new InvalidOperationException("Only Active policies can be expired.");

        policy.Status = PolicyStatus.Expired;
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> RenewAsync(Guid id, DateTime newEndDate)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy == null) return null;
        if (policy.Status != PolicyStatus.Expired)
            throw new InvalidOperationException("Only Expired policies can be renewed.");

        // Create a new policy version for the renewal
        var renewedPolicy = new Policy
        {
            PolicyNumber = policy.PolicyNumber,
            CustomerId = policy.CustomerId,
            PolicyType = policy.PolicyType,
            Status = PolicyStatus.Active,
            CoverageAmount = policy.CoverageAmount,
            Premium = policy.Premium,
            StartDate = DateTime.UtcNow,
            EndDate = newEndDate,
            IsAutoRenewal = policy.IsAutoRenewal,
            OriginalPolicyId = policy.OriginalPolicyId ?? policy.Id,
            Version = policy.Version + 1
        };

        _context.Policies.Add(renewedPolicy);
        await _context.SaveChangesAsync();
        return renewedPolicy;
    }

    public async Task<PolicyEndorsement> AddEndorsementAsync(Guid policyId, PolicyEndorsement endorsement)
    {
        var policy = await _context.Policies.FindAsync(policyId);
        if (policy == null) throw new InvalidOperationException("Policy not found.");
        if (policy.Status != PolicyStatus.Active)
            throw new InvalidOperationException("Endorsements can only be added to Active policies.");

        endorsement.PolicyId = policyId;

        // Apply endorsement changes to policy
        switch (endorsement.Type)
        {
            case EndorsementType.CoverageIncrease:
            case EndorsementType.CoverageDecrease:
                if (endorsement.NewValue.HasValue)
                    policy.CoverageAmount = endorsement.NewValue.Value;
                break;
            case EndorsementType.PremiumIncrease:
            case EndorsementType.PremiumDecrease:
                if (endorsement.NewValue.HasValue)
                    policy.Premium = endorsement.NewValue.Value;
                break;
            case EndorsementType.TermExtension:
                if (endorsement.NewValue.HasValue)
                    policy.EndDate = policy.EndDate?.AddDays((double)endorsement.NewValue.Value);
                break;
        }

        _context.PolicyEndorsements.Add(endorsement);
        await _context.SaveChangesAsync();
        return endorsement;
    }

    public async Task<List<EndorsementDto>> GetEndorsementsAsync(Guid policyId)
    {
        return await _context.PolicyEndorsements
            .AsNoTracking()
            .Where(e => e.PolicyId == policyId)
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => new EndorsementDto
            {
                Id = e.Id,
                PolicyId = e.PolicyId,
                Type = e.Type,
                Description = e.Description,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                EffectiveDate = e.EffectiveDate,
                CreatedDate = e.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<List<PolicyDto>> GetByStatusAsync(PolicyStatus status)
    {
        return await _context.Policies
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Endorsements)
            .Where(p => p.Status == status)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PolicyDto>> GetExpiredAsync()
    {
        return await GetByStatusAsync(PolicyStatus.Expired);
    }

    public async Task<List<PolicyDto>> GetUpForRenewalAsync(int daysBeforeExpiry = 7)
    {
        var threshold = DateTime.UtcNow.AddDays(daysBeforeExpiry);
        return await _context.Policies
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Endorsements)
            .Where(p => p.Status == PolicyStatus.Active
                     && p.EndDate <= threshold
                     && p.EndDate > DateTime.UtcNow)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PolicyDto>> GetPendingAutoRenewalAsync()
    {
        return await _context.Policies
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Endorsements)
            .Where(p => p.Status == PolicyStatus.Expired
                     && p.IsAutoRenewal
                     && p.RenewalReminderSent == false)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }
}

