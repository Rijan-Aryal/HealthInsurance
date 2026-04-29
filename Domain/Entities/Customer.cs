using Domain.Enums;
using System.Collections.Generic;

namespace Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string CitizenshipNumber { get; set; } = string.Empty; // Nepal specific
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}

