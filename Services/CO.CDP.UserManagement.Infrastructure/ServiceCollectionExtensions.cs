using CO.CDP.AwsServices;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CO.CDP.UserManagement.Infrastructure;

/// <summary>
/// Extension methods for configuring user management infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds user management infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="useSharedConnection">
    /// When true, resolves a scoped <c>NpgsqlConnection</c> from DI (for atomic dual-write via <c>IAtomicMembershipSync</c>).
    /// When false, creates a new connection from the connection string.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserManagementInfrastructure(
        this IServiceCollection services,
        string connectionString,
        bool useSharedConnection = false)
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<UserManagementDbContext>((sp, options) =>
        {
            if (useSharedConnection)
                options.UseNpgsql(
                    sp.GetRequiredService<NpgsqlConnection>(),
                    npgsql => npgsql
                        .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            else
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql
                        .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<ISlugGeneratorService, SlugGeneratorService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IOrganisationApplicationService, OrganisationApplicationService>();
        services.AddScoped<IUserAssignmentService, UserAssignmentService>();
        services.AddScoped<IOrganisationUserService, OrganisationUserService>();
        services.AddScoped<IOrganisationRoleService, OrganisationRoleService>();
        services.AddScoped<IRoleMappingService, RoleMappingService>();
        services.AddScoped<IOrganisationPersonSyncRepository, OrganisationPersonSyncRepository>();
        services.AddScoped<IInviteOrchestrationService, InviteOrchestrationService>();
        services.AddScoped<IJoinRequestOrchestrationService, JoinRequestOrchestrationService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IMembershipAuthorizationGuard, MembershipAuthorizationGuard>();

        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOrganisationApplicationRepository, OrganisationApplicationRepository>();
        services.AddScoped<IUserOrganisationMembershipRepository, UserOrganisationMembershipRepository>();
        services.AddScoped<IUserApplicationAssignmentRepository, UserApplicationAssignmentRepository>();
        services.AddScoped<IInviteRoleMappingRepository, InviteRoleMappingRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Registers the minimal UM services needed to sync organisations from another service
    /// (e.g. Organisation WebApi) directly into the UserManagement database.
    /// </summary>
    public static IServiceCollection AddUserManagementOrganisationSync(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<UserManagementDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsql => npgsql
                    .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IOrganisationApplicationRepository, OrganisationApplicationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserOrganisationMembershipRepository, UserOrganisationMembershipRepository>();
        services.AddScoped<IUserApplicationAssignmentRepository, UserApplicationAssignmentRepository>();
        services.AddScoped<ISlugGeneratorService, SlugGeneratorService>();
        services.AddScoped<IUmOrganisationSyncRepository, UmOrganisationSyncRepository>();

        return services;
    }

    /// <summary>
    /// Adds caching services for user management using ElastiCache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="awsConfiguration">AWS configuration containing ElastiCache settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserManagementCaching(
        this IServiceCollection services,
        AwsConfiguration? awsConfiguration = null)
    {
        if (awsConfiguration?.ElastiCache is null)
        {
            throw new InvalidOperationException(
                "ElastiCache is not configured. Ensure Aws:ElastiCache:Hostname and Aws:ElastiCache:Port are set.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{awsConfiguration.ElastiCache.Hostname}:{awsConfiguration.ElastiCache.Port}";
            options.InstanceName = "UserManagement_";
        });

        services.AddScoped<IClaimsCacheService, CachedClaimsService>();

        return services;
    }
}