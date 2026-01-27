using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;
using CO.CDP.ApplicationRegistry.Infrastructure.Repositories;
using CO.CDP.ApplicationRegistry.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.ApplicationRegistry.Infrastructure;

/// <summary>
/// Extension methods for configuring application registry infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application registry infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cdpConnectionString">Optional CDP database connection string for organisation lookups.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationRegistryInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string? cdpConnectionString = null)
    {
        // Register DbContext
        services.AddDbContext<ApplicationRegistryDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationRegistryDbContext).Assembly.FullName)));

        // Register services
        services.AddScoped<ISlugGeneratorService, SlugGeneratorService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IOrganisationApplicationService, OrganisationApplicationService>();
        services.AddScoped<IUserAssignmentService, UserAssignmentService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register repositories
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOrganisationApplicationRepository, OrganisationApplicationRepository>();
        services.AddScoped<IUserOrganisationMembershipRepository, UserOrganisationMembershipRepository>();
        services.AddScoped<IUserApplicationAssignmentRepository, UserApplicationAssignmentRepository>();

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds caching services for the application registry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">Optional Redis connection string. If null, uses in-memory cache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationRegistryCaching(
        this IServiceCollection services,
        string? redisConnectionString = null)
    {
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            // Add Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ApplicationRegistry_";
            });
        }
        else
        {
            // Add in-memory cache as fallback
            services.AddDistributedMemoryCache();
        }

        // Register cache service
        services.AddScoped<IClaimsCacheService, CachedClaimsService>();

        return services;
    }
}
