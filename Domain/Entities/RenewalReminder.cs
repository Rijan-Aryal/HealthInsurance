using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class RenewalReminder : BaseEntity
{
    [Required]
    public Guid PolicyId { get; set; }

    public Policy Policy { get; set; } = null!;

    public DateTime ReminderDate { get; set; }

    public bool IsSent { get; set; } = false;

    public DateTime? SentDate { get; set; }

    public bool IsAcknowledged { get; set; } = false;
}

