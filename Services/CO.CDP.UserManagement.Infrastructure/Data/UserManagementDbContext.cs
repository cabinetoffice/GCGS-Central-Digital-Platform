using CO.CDP.UserManagement.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;

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

    public DbSet<CoreEntities.Organisation> Organisations => Set<CoreEntities.Organisation>();
    public DbSet<CoreEntities.Application> Applications => Set<CoreEntities.Application>();
    public DbSet<CoreEntities.ApplicationPermission> ApplicationPermissions => Set<CoreEntities.ApplicationPermission>();
    public DbSet<CoreEntities.ApplicationRole> ApplicationRoles => Set<CoreEntities.ApplicationRole>();
    public DbSet<CoreEntities.UserOrganisationMembership> UserOrganisationMemberships => Set<CoreEntities.UserOrganisationMembership>();
    public DbSet<CoreEntities.PendingOrganisationInvite> PendingOrganisationInvites => Set<CoreEntities.PendingOrganisationInvite>();
    public DbSet<CoreEntities.OrganisationApplication> OrganisationApplications => Set<CoreEntities.OrganisationApplication>();
    public DbSet<CoreEntities.UserApplicationAssignment> UserApplicationAssignments => Set<CoreEntities.UserApplicationAssignment>();

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
        modelBuilder.Entity<CoreEntities.Organisation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.Application>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.ApplicationPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.ApplicationRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.UserOrganisationMembership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.PendingOrganisationInvite>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.OrganisationApplication>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CoreEntities.UserApplicationAssignment>().HasQueryFilter(e => !e.IsDeleted);
    }
}
