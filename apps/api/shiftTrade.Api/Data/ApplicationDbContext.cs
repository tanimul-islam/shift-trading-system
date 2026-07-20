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
    }

          
}