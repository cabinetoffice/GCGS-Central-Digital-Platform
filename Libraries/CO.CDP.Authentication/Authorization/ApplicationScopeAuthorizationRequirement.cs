using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

public class ApplicationScopeAuthorizationRequirement(
    string clientId,
    string[]? applicationRoles = null,
    string[]? applicationPermissions = null) : IAuthorizationRequirement
{
    public string ClientId { get; } = clientId;
    public string[] ApplicationRoles { get; } = applicationRoles ?? [];
    public string[] ApplicationPermissions { get; } = applicationPermissions ?? [];
}
