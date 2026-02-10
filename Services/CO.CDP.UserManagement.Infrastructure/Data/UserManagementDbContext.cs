using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using OrganisationEntity = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Data;

/// <summary>
/// Database context for User Management.
/// </summary>
public class UserManagementDbContext : DbContext
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrganisationEntity> Organisations => Set<OrganisationEntity>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<UserOrganisationMembership> UserOrganisationMemberships => Set<UserOrganisationMembership>();
    public DbSet<PendingOrganisationInvite> PendingOrganisationInvites => Set<PendingOrganisationInvite>();
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
        modelBuilder.ApplyConfiguration(new PendingOrganisationInviteConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new UserApplicationAssignmentConfiguration());

        // Global query filters for soft delete
        modelBuilder.Entity<OrganisationEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Application>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApplicationPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApplicationRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserOrganisationMembership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PendingOrganisationInvite>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganisationApplication>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserApplicationAssignment>().HasQueryFilter(e => !e.IsDeleted);
    }
}
