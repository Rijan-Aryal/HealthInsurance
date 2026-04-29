using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Policy> Policies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<PolicyEndorsement> PolicyEndorsements { get; set; }
    public DbSet<RenewalReminder> RenewalReminders { get; set; }
    public DbSet<ClaimDocument> ClaimDocuments { get; set; }
    public DbSet<ClaimAuditLog> ClaimAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>()
            .HasOne<Policy>(c => c.Policy)
            .WithMany(p => p.Claims)
            .HasForeignKey(c => c.PolicyId);

        modelBuilder.Entity<ClaimDocument>()
            .HasOne<Claim>(d => d.Claim)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClaimAuditLog>()
            .HasOne<Claim>(a => a.Claim)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(a => a.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Policy>()
            .HasOne<Customer>(p => p.Customer)
            .WithMany(c => c.Policies)
            .HasForeignKey(p => p.CustomerId)
            .IsRequired();

        modelBuilder.Entity<PolicyEndorsement>()
            .HasOne<Policy>(e => e.Policy)
            .WithMany(p => p.Endorsements)
            .HasForeignKey(e => e.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RenewalReminder>()
            .HasOne<Policy>(r => r.Policy)
            .WithMany(p => p.RenewalReminders)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision
        modelBuilder.Entity<Claim>()
            .Property(c => c.ClaimAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Claim>()
            .Property(c => c.ApprovedAmount)
            .HasPrecision(18, 2);

        base.OnModelCreating(modelBuilder);
    }
}

