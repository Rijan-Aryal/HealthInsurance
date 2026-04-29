using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PolicyLifecycleBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PolicyLifecycleBackgroundService> _logger;

    public PolicyLifecycleBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PolicyLifecycleBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Policy Lifecycle Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var policyRepo = scope.ServiceProvider.GetRequiredService<IPolicyRepository>();
                var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.AppDbContext>();

                await ExpirePoliciesAsync(policyRepo, context, stoppingToken);
                await SendRenewalRemindersAsync(policyRepo, context, stoppingToken);
                await AutoRenewPoliciesAsync(policyRepo, context, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Policy Lifecycle Background Service.");
            }

            // Run once per day
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ExpirePoliciesAsync(IPolicyRepository repo, Infrastructure.Data.AppDbContext context, CancellationToken ct)
    {
        var expiredPolicies = await context.Policies
            .Where(p => p.Status == PolicyStatus.Active && p.EndDate < DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var policy in expiredPolicies)
        {
            await repo.ExpireAsync(policy.Id);
            _logger.LogInformation("Policy {PolicyNumber} has been expired.", policy.PolicyNumber);
        }
    }

    private async Task SendRenewalRemindersAsync(IPolicyRepository repo, Infrastructure.Data.AppDbContext context, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddDays(7);
        var policies = await context.Policies
            .Include(p => p.Customer)
            .Where(p => p.Status == PolicyStatus.Active
                     && p.EndDate <= threshold
                     && p.EndDate > DateTime.UtcNow
                     && !p.RenewalReminderSent)
            .ToListAsync(ct);

        foreach (var policy in policies)
        {
            policy.RenewalReminderSent = true;

            var reminder = new RenewalReminder
            {
                PolicyId = policy.Id,
                ReminderDate = DateTime.UtcNow,
                IsSent = true,
                SentDate = DateTime.UtcNow
            };
            context.RenewalReminders.Add(reminder);

            _logger.LogInformation(
                "Renewal reminder sent for policy {PolicyNumber} to customer {CustomerEmail}.",
                policy.PolicyNumber,
                policy.Customer.Email);
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task AutoRenewPoliciesAsync(IPolicyRepository repo, Infrastructure.Data.AppDbContext context, CancellationToken ct)
    {
        var expiredPolicies = await context.Policies
            .Where(p => p.Status == PolicyStatus.Expired
                     && p.IsAutoRenewal
                     && p.RenewalReminderSent)
            .ToListAsync(ct);

        foreach (var policy in expiredPolicies)
        {
            await repo.RenewAsync(policy.Id, DateTime.UtcNow.AddYears(1));
            _logger.LogInformation("Policy {PolicyNumber} has been auto-renewed.", policy.PolicyNumber);
        }
    }
}

