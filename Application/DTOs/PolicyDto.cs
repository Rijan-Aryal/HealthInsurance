using Domain.Enums;

namespace Application.DTOs;

public class PolicyDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public CustomerDto Customer { get; set; } = null!;
    public PolicyType PolicyType { get; set; }
    public PolicyStatus Status { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Premium { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAutoRenewal { get; set; }
    public bool RenewalReminderSent { get; set; }
    public Guid? OriginalPolicyId { get; set; }
    public int Version { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<EndorsementDto> Endorsements { get; set; } = new();
}

public class CreatePolicyDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public PolicyType PolicyType { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Premium { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAutoRenewal { get; set; } = false;
}

public class UpdatePolicyDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public PolicyType PolicyType { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Premium { get; set; }
}

public class RenewPolicyDto
{
    public DateTime NewEndDate { get; set; }
}

public class PolicyStatusUpdateDto
{
    public string Reason { get; set; } = string.Empty;
}

