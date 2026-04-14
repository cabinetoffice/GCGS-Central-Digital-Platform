using System.Text.Json;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public sealed class RemoveInviteSessionStore(IHttpContextAccessor httpContextAccessor) : IRemoveInviteStateStore
{
    private const string RemoveInviteSuccessStateKey = "RemoveInviteSuccessState";

    public Task<RemoveInviteSuccessState?> GetAsync()
    {
        var session = GetSession();
        var stateJson = session.GetString(RemoveInviteSuccessStateKey);
        return Task.FromResult(string.IsNullOrEmpty(stateJson)
            ? null
            : JsonSerializer.Deserialize<RemoveInviteSuccessState>(stateJson));
    }

    public Task SetAsync(RemoveInviteSuccessState state)
    {
        var session = GetSession();
        session.SetString(RemoveInviteSuccessStateKey, JsonSerializer.Serialize(state));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var session = GetSession();
        session.Remove(RemoveInviteSuccessStateKey);
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
