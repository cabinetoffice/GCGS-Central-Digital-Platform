using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;

public static class AuthorizationPolicies
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string OrgAdmin = "OrgAdmin";
    public const string OrgMember = "OrgMember";

    public static IServiceCollection AddApplicationRegistryAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PlatformAdmin, policy =>
                policy.Requirements.Add(new PlatformAdminRequirement()));

            options.AddPolicy(OrgAdmin, policy =>
                policy.Requirements.Add(new OrganisationRoleRequirement("Admin", "Owner")));

            options.AddPolicy(OrgMember, policy =>
                policy.Requirements.Add(new OrganisationRoleRequirement("Member", "Admin", "Owner")));
        });

        services.AddScoped<IAuthorizationHandler, PlatformAdminHandler>();
        services.AddScoped<IAuthorizationHandler, OrganisationRoleHandler>();

        return services;
    }
}

public class PlatformAdminRequirement : IAuthorizationRequirement { }

public class OrganisationRoleRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }

    public OrganisationRoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

public class PlatformAdminHandler : AuthorizationHandler<PlatformAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformAdminRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "platform_role" && c.Value == "admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class OrganisationRoleHandler : AuthorizationHandler<OrganisationRoleRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganisationRoleHandler(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationRoleRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "platform_role" && c.Value == "admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (!httpContext.Request.RouteValues.TryGetValue("orgId", out var orgIdValue)
            || !Guid.TryParse(orgIdValue?.ToString(), out var orgId))
        {
            return;
        }

        var urn = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(urn))
        {
            return;
        }

        // Fast path: check JWT org-role claim (org:{orgId}:role=<role>).
        // These claims are injected by the authority token-enrichment pipeline.
        var claimType = $"org:{orgId}:role";
        var orgRoleClaim = context.User.FindFirst(c => c.Type == claimType);
        if (orgRoleClaim != null
            && requirement.AllowedRoles.Contains(orgRoleClaim.Value, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // Fallback: verify membership via the AppRegistry database.
        using var scope = _serviceProvider.CreateScope();
        var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganisationRepository>();

        var membership = await orgRepo.GetMemberAsync(orgId, urn);
        if (membership != null
            && membership.IsActive
            && requirement.AllowedRoles.Contains(membership.OrganisationRole, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}
