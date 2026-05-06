using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence;

public class ApplicationRegistryContext : DbContext
{
    public ApplicationRegistryContext(DbContextOptions<ApplicationRegistryContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserOrganisationMembership> UserOrganisationMemberships => Set<UserOrganisationMembership>();
    public DbSet<OrganisationApplication> OrganisationApplications => Set<OrganisationApplication>();
    public DbSet<UserApplicationAssignment> UserApplicationAssignments => Set<UserApplicationAssignment>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<FeatureFlagScope> FeatureFlagScopes => Set<FeatureFlagScope>();
    public DbSet<ReportCategory> ReportCategories => Set<ReportCategory>();
    public DbSet<CategoryPermission> CategoryPermissions => Set<CategoryPermission>();
    public DbSet<ReportCategoryAssignment> ReportCategoryAssignments => Set<ReportCategoryAssignment>();
    public DbSet<AccessControlEntry> AccessControlEntries => Set<AccessControlEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Organisation
        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(64);
        });

        // Application
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.Category).HasMaxLength(128);
        });

        // ApplicationPermission
        modelBuilder.Entity<ApplicationPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ApplicationId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.HasOne(e => e.Application)
                .WithMany(a => a.Permissions)
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApplicationRole
        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ApplicationId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.HasOne(e => e.Application)
                .WithMany(a => a.Roles)
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RolePermission (composite key)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserOrganisationMembership
        modelBuilder.Entity<UserOrganisationMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserPrincipalId, e.OrganisationId }).IsUnique();
            entity.Property(e => e.UserPrincipalId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.OrganisationRole).IsRequired().HasMaxLength(64);
            entity.HasOne(e => e.Organisation)
                .WithMany(o => o.Members)
                .HasForeignKey(e => e.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrganisationApplication (composite key)
        modelBuilder.Entity<OrganisationApplication>(entity =>
        {
            entity.HasKey(e => new { e.OrganisationId, e.ApplicationId });
            entity.Property(e => e.EnabledBy).IsRequired().HasMaxLength(256);
            entity.HasOne(e => e.Organisation)
                .WithMany(o => o.Applications)
                .HasForeignKey(e => e.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Application)
                .WithMany()
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserApplicationAssignment
        modelBuilder.Entity<UserApplicationAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserPrincipalId, e.ApplicationId, e.OrganisationId }).IsUnique();
            entity.Property(e => e.UserPrincipalId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.AssignedBy).IsRequired().HasMaxLength(256);
            entity.HasOne(e => e.Application)
                .WithMany()
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Organisation)
                .WithMany()
                .HasForeignKey(e => e.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserRoleAssignment (composite key)
        modelBuilder.Entity<UserRoleAssignment>(entity =>
        {
            entity.HasKey(e => new { e.UserApplicationAssignmentId, e.RoleId });
            entity.HasOne(e => e.UserApplicationAssignment)
                .WithMany(a => a.RoleAssignments)
                .HasForeignKey(e => e.UserApplicationAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FeatureFlag
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TileId).IsUnique();
            entity.Property(e => e.TileId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Reason).HasMaxLength(512);
        });

        // FeatureFlagScope
        modelBuilder.Entity<FeatureFlagScope>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FeatureFlagId, e.OrganisationTypeId }).IsUnique();
            entity.HasOne(e => e.FeatureFlag)
                .WithMany(f => f.Scopes)
                .HasForeignKey(e => e.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReportCategory
        modelBuilder.Entity<ReportCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.TaxonomyType).IsRequired().HasMaxLength(64);
        });

        // CategoryPermission
        modelBuilder.Entity<CategoryPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CategoryId, e.OrganisationTypeId }).IsUnique();
            entity.Property(e => e.PermissionLevel).IsRequired().HasMaxLength(32);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Permissions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReportCategoryAssignment
        modelBuilder.Entity<ReportCategoryAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ReportId, e.CategoryId }).IsUnique();
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Assignments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AccessControlEntry
        modelBuilder.Entity<AccessControlEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ReportId, e.UserPrincipal, e.OrganisationId });
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(32);
            entity.Property(e => e.PropertyName).HasMaxLength(256);
            entity.Property(e => e.OldValue).HasMaxLength(4096);
            entity.Property(e => e.NewValue).HasMaxLength(4096);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);
        });
    }
}
