using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using shiftTrade.api.models;

namespace shiftTrade.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public DbSet<Shifts> Shifts => Set<Shifts>();
    public DbSet<HoursDebt> HoursDebts => Set<HoursDebt>();
    public DbSet<DebtSettlement> DebtSettlements => Set<DebtSettlement>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity <Location>()
        .HasOne(location => location.Organization)
        .WithMany()
        .HasForeignKey(location => location.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);
        

        builder.Entity<OrganizationMembership>(entity =>
        {
            entity.HasIndex(membership => new
            {
                membership.OrganizationId,
                membership.userId

            })
            .IsUnique();

            entity.HasOne(membership => membership.Organization)
            .WithMany()
            .HasForeignKey(membership => membership.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


            entity.HasOne(membership => membership.User)
            .WithMany()
            .HasForeignKey(membership => membership.userId)
            .OnDelete(DeleteBehavior.Restrict);
        });


        builder.Entity<HoursDebt>(entity =>
        {
            entity.HasIndex(debt => debt.ShiftId).IsUnique();

            entity.Property(debt =>debt.HoursOwed).HasPrecision(5,2);

             entity.Property(debt => debt.RemainingHours)
            .HasPrecision(5, 2);
        });

        builder.Entity<DebtSettlement>(entity =>
        {
            entity.Property(settlement => settlement.HoursApplied)
                .HasPrecision(5, 2);
        });
    }

          
}