using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data;

/// <summary>
/// Database context for the Application Registry.
/// </summary>
public class ApplicationRegistryDbContext : DbContext
{
    public ApplicationRegistryDbContext(DbContextOptions<ApplicationRegistryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<UserOrganisationMembership> UserOrganisationMemberships => Set<UserOrganisationMembership>();
    public DbSet<OrganisationApplication> OrganisationApplications => Set<OrganisationApplication>();
    public DbSet<UserApplicationAssignment> UserApplicationAssignments => Set<UserApplicationAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new OrganisationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganisationMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new UserApplicationAssignmentConfiguration());

        // Global query filters for soft delete
        modelBuilder.Entity<Organisation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Application>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApplicationPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApplicationRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserOrganisationMembership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganisationApplication>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserApplicationAssignment>().HasQueryFilter(e => !e.IsDeleted);
    }
}
