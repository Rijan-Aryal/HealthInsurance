using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly AppDbContext _context;

    public ClaimRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Claim>> GetAllAsync()
    {
        return await _context.Claims
            .Include(c => c.Policy)
            .Include(c => c.Documents)
            .Include(c => c.AuditLogs)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Claim>> GetByPolicyIdAsync(Guid policyId)
    {
        return await _context.Claims
            .Where(c => c.PolicyId == policyId)
            .Include(c => c.Documents)
            .Include(c => c.AuditLogs)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Claim>> GetByStatusAsync(ClaimStatus status)
    {
        return await _context.Claims
            .Where(c => c.Status == status)
            .Include(c => c.Policy)
            .Include(c => c.Documents)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<Claim?> GetByIdAsync(Guid id)
    {
        return await _context.Claims
            .Include(c => c.Policy)
            .Include(c => c.Documents)
            .Include(c => c.AuditLogs)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Claim> AddAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task UpdateAsync(Claim claim)
    {
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var claim = await GetByIdAsync(id);
        if (claim != null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddAuditLogAsync(ClaimAuditLog auditLog)
    {
        _context.ClaimAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ClaimAuditLog>> GetAuditLogsAsync(Guid claimId)
    {
        return await _context.ClaimAuditLogs
            .Where(a => a.ClaimId == claimId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task AddDocumentAsync(ClaimDocument document)
    {
        _context.ClaimDocuments.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ClaimDocument>> GetDocumentsAsync(Guid claimId)
    {
        return await _context.ClaimDocuments
            .Where(d => d.ClaimId == claimId)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public Task<bool> CanTransitionAsync(ClaimStatus fromStatus, ClaimStatus toStatus)
    {
        var validTransitions = new Dictionary<ClaimStatus, ClaimStatus[]>
        {
            [ClaimStatus.Submitted] = new[] { ClaimStatus.UnderReview, ClaimStatus.Rejected },
            [ClaimStatus.UnderReview] = new[] { ClaimStatus.Approved, ClaimStatus.Rejected },
            [ClaimStatus.Approved] = new[] { ClaimStatus.Paid, ClaimStatus.Rejected },
            [ClaimStatus.Rejected] = Array.Empty<ClaimStatus>(),
            [ClaimStatus.Paid] = Array.Empty<ClaimStatus>()
        };

        return Task.FromResult(validTransitions.ContainsKey(fromStatus) && validTransitions[fromStatus].Contains(toStatus));
    }
}

