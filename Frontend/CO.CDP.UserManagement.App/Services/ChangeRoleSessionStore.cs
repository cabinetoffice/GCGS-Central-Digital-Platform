using System.Text.Json;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public sealed class ChangeRoleSessionStore(IHttpContextAccessor httpContextAccessor) : IChangeRoleStateStore
{
    private const string ChangeRoleStateKey = "ChangeRoleState";

    public Task<ChangeRoleState?> GetAsync()
    {
        var session = GetSession();
        var stateJson = session.GetString(ChangeRoleStateKey);
        return Task.FromResult(string.IsNullOrEmpty(stateJson)
            ? null
            : JsonSerializer.Deserialize<ChangeRoleState>(stateJson));
    }

    public Task SetAsync(ChangeRoleState state)
    {
        var session = GetSession();
        session.SetString(ChangeRoleStateKey, JsonSerializer.Serialize(state));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var session = GetSession();
        session.Remove(ChangeRoleStateKey);
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
