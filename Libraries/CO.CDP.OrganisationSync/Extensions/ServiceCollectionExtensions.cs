namespace CO.CDP.OrganisationSync;

using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Infrastructure.Services;
using UmIOrganisationRepository = CO.CDP.UserManagement.Core.Interfaces.IOrganisationRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers shared kernel services for atomic OI ↔ UM dual-writes.
    /// Both <see cref="OrganisationInformationContext"/> and <see cref="UserManagementDbContext"/>
    /// share a single scoped <see cref="NpgsqlConnection"/> so that <see cref="AtomicScope"/>
    /// can enlist both in the same PostgreSQL transaction.
    /// </summary>
    public static IServiceCollection AddOrganisationMembershipSync(
        this IServiceCollection services,
        string connectionString)
    {
        // Shared NpgsqlDataSource (singleton — may already be registered by the host)
        services.TryAddSingleton(
            new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());

        // One connection per request, shared by both DbContexts
        services.TryAddScoped(sp =>
            sp.GetRequiredService<NpgsqlDataSource>().CreateConnection());

        // OI context on the shared connection
        services.AddDbContext<OrganisationInformationContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));

        // UM context on the same shared connection
        services.AddDbContext<UserManagementDbContext>((sp, options) =>
            options.UseNpgsql(
                sp.GetRequiredService<NpgsqlConnection>(),
                npgsql => npgsql
                    .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        // UM repositories
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<UmIOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IOrganisationApplicationRepository, OrganisationApplicationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserOrganisationMembershipRepository, UserOrganisationMembershipRepository>();
        services.AddScoped<IUserApplicationAssignmentRepository, UserApplicationAssignmentRepository>();
        services.AddScoped<ISlugGeneratorService, SlugGeneratorService>();
        services.AddScoped<IUmOrganisationSyncRepository, UmOrganisationSyncRepository>();

        // Shared kernel
        services.AddScoped<IAtomicScope, AtomicScope>();
        services.AddScoped<IOrganisationMembershipSync, OrganisationMembershipSync>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IAtomicMembershipSync"/> and its dependencies for use in the
    /// UserManagement API. DbContexts and UM repositories are expected to be already registered
    /// (e.g. via <c>AddUserManagementInfrastructure</c>). Uses <c>TryAddScoped</c> throughout.
    /// </summary>
    public static IServiceCollection AddAtomicMembershipSync(this IServiceCollection services)
    {
        services.TryAddScoped<IAtomicScope, AtomicScope>();
        services.TryAddScoped<IOrganisationMembershipSync, OrganisationMembershipSync>();
        services.TryAddScoped<IUmOrganisationSyncRepository, UmOrganisationSyncRepository>();
        services.TryAddScoped<IAtomicMembershipSync, AtomicMembershipSync>();
        return services;
    }
}
