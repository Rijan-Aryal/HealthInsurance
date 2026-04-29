using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ClaimProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClaimProcessingBackgroundService> _logger;

    public ClaimProcessingBackgroundService(IServiceProvider serviceProvider, ILogger<ClaimProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Claim Processing Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await ValidateSubmittedClaimsAsync(dbContext, stoppingToken);
                await SendApprovalNotificationsAsync(dbContext, stoppingToken);
                await FlagHighValueClaimsAsync(dbContext, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Claim Processing Background Service.");
            }

            // Run every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task ValidateSubmittedClaimsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        // Auto-move Submitted claims that have documents to UnderReview after 1 hour
        var threshold = DateTime.UtcNow.AddHours(-1);
        
        var claimsToReview = await dbContext.Claims
            .Where(c => c.Status == ClaimStatus.Submitted && c.CreatedDate <= threshold)
            .Include(c => c.Documents)
            .ToListAsync(cancellationToken);

        foreach (var claim in claimsToReview)
        {
            // Only move to UnderReview if documents are attached
            if (claim.Documents?.Any() == true)
            {
                var oldStatus = claim.Status;
                claim.Status = ClaimStatus.UnderReview;
                
                dbContext.ClaimAuditLogs.Add(new Domain.Entities.ClaimAuditLog
                {
                    ClaimId = claim.Id,
                    FromStatus = oldStatus,
                    ToStatus = ClaimStatus.UnderReview,
                    Action = "Auto-Validation: Documents verified, moved to UnderReview",
                    PerformedBy = "System",
                    Notes = "Background job validated submitted claim after document check"
                });

                _logger.LogInformation("Claim {ClaimId} auto-moved to UnderReview", claim.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SendApprovalNotificationsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        // Find claims UnderReview for more than 2 days - flag for urgent review
        var urgentThreshold = DateTime.UtcNow.AddDays(-2);
        
        var urgentClaims = await dbContext.Claims
            .Where(c => c.Status == ClaimStatus.UnderReview && c.CreatedDate <= urgentThreshold)
            .ToListAsync(cancellationToken);

        foreach (var claim in urgentClaims)
        {
            if (string.IsNullOrEmpty(claim.AssignedAdjuster))
            {
                // Auto-assign a default adjuster for urgent claims
                claim.AssignedAdjuster = "Unassigned - Urgent Review Needed";
                
                dbContext.ClaimAuditLogs.Add(new Domain.Entities.ClaimAuditLog
                {
                    ClaimId = claim.Id,
                    FromStatus = ClaimStatus.UnderReview,
                    ToStatus = ClaimStatus.UnderReview,
                    Action = "Urgent Review Flagged",
                    PerformedBy = "System",
                    Notes = "Claim has been under review for over 2 days without adjuster assignment"
                });

                _logger.LogWarning("Claim {ClaimId} flagged for urgent review - no adjuster assigned", claim.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task FlagHighValueClaimsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        // Flag high-value claims (above 100,000) for extra scrutiny
        var highValueClaims = await dbContext.Claims
            .Where(c => c.Status == ClaimStatus.Submitted && c.ClaimAmount > 100000)
            .ToListAsync(cancellationToken);

        foreach (var claim in highValueClaims)
        {
            if (!claim.IsFlaggedForReview)
            {
                claim.IsFlaggedForReview = true;
                claim.FlagReason = $"High value claim: {claim.ClaimAmount:C} exceeds threshold. Requires manual fraud review.";
                
                dbContext.ClaimAuditLogs.Add(new Domain.Entities.ClaimAuditLog
                {
                    ClaimId = claim.Id,
                    FromStatus = ClaimStatus.Submitted,
                    ToStatus = ClaimStatus.Submitted,
                    Action = "High Value Claim Flagged",
                    PerformedBy = "System",
                    Notes = claim.FlagReason
                });

                _logger.LogWarning("High value claim {ClaimId} flagged for fraud review: {Amount}", claim.Id, claim.ClaimAmount);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

