using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for accessing current user information from the HTTP context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserPrincipalId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        // Try common claim types for user identifier
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst("user_id")?.Value;
    }

    public string? GetUserEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public string? GetCurrentUserId()
    {
        return GetUserPrincipalId();
    }
}
