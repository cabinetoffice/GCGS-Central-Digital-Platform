using System.Text.Json;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public sealed class InviteUserSessionStore(IHttpContextAccessor httpContextAccessor) : IInviteUserStateStore
{
    private const string InviteUserStateKey = "InviteUserState";

    public Task<InviteUserState?> GetAsync()
    {
        var session = GetSession();
        var stateJson = session.GetString(InviteUserStateKey);
        return Task.FromResult(string.IsNullOrEmpty(stateJson)
            ? null
            : JsonSerializer.Deserialize<InviteUserState>(stateJson));
    }

    public Task SetAsync(InviteUserState state)
    {
        var session = GetSession();
        session.SetString(InviteUserStateKey, JsonSerializer.Serialize(state));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var session = GetSession();
        session.Remove(InviteUserStateKey);
        return Task.CompletedTask;
    }

    private ISession GetSession()
    {
        if (httpContextAccessor.HttpContext?.Session == null)
        {
            throw new InvalidOperationException("Session is not available. Ensure session middleware is configured.");
        }
        return httpContextAccessor.HttpContext.Session;
    }
}
