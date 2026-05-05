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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganisationRoleHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationRoleRequirement requirement)
    {
        // Platform admins always pass org-level checks
        if (context.User.HasClaim(c => c.Type == "platform_role" && c.Value == "admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }

        // Check org role claims
        var orgRoleClaims = context.User.FindAll("org_role");
        foreach (var claim in orgRoleClaims)
        {
            if (requirement.AllowedRoles.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
