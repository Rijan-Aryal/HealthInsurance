using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class Policy : BaseEntity
{
    [Required]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Required]
    public PolicyType PolicyType { get; set; }

    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public decimal CoverageAmount { get; set; }

    public decimal Premium { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsAutoRenewal { get; set; } = false;

    public bool RenewalReminderSent { get; set; } = false;

    public Guid? OriginalPolicyId { get; set; }

    public int Version { get; set; } = 1;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public ICollection<PolicyEndorsement> Endorsements { get; set; } = new List<PolicyEndorsement>();

    public ICollection<RenewalReminder> RenewalReminders { get; set; } = new List<RenewalReminder>();
}

