using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

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
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserManagementInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<UserManagementDbContext>((sp, options) =>
            options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<ISlugGeneratorService, SlugGeneratorService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IOrganisationApplicationService, OrganisationApplicationService>();
        services.AddScoped<IUserAssignmentService, UserAssignmentService>();
        services.AddScoped<IOrganisationUserService, OrganisationUserService>();
        services.AddScoped<IInviteOrchestrationService, InviteOrchestrationService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPersonLookupService, PersonLookupService>();
        services.AddScoped<IOrganisationPersonsSyncService, OrganisationPersonsSyncService>();
        services.AddScoped<IOrganisationSyncService, OrganisationSyncService>();

        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOrganisationApplicationRepository, OrganisationApplicationRepository>();
        services.AddScoped<IUserOrganisationMembershipRepository, UserOrganisationMembershipRepository>();
        services.AddScoped<IPendingOrganisationInviteRepository, PendingOrganisationInviteRepository>();
        services.AddScoped<IUserApplicationAssignmentRepository, UserApplicationAssignmentRepository>();
        services.AddScoped<IInviteRoleMappingRepository, InviteRoleMappingRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds caching services for user management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">Optional Redis connection string. If null, uses in-memory cache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserManagementCaching(
        this IServiceCollection services,
        string? redisConnectionString = null)
    {
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UserManagement_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IClaimsCacheService, CachedClaimsService>();

        return services;
    }
}
