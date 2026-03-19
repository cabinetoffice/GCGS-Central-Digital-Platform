using System.Text.Json;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public sealed class ChangeApplicationRoleSessionStore(IHttpContextAccessor httpContextAccessor) : IChangeApplicationRoleStateStore
{
    private const string ChangeApplicationRoleStateKey = "ChangeApplicationRoleState";

    public Task<ChangeApplicationRoleState?> GetAsync()
    {
        var session = GetSession();
        var stateJson = session.GetString(ChangeApplicationRoleStateKey);
        return Task.FromResult(string.IsNullOrEmpty(stateJson)
            ? null
            : JsonSerializer.Deserialize<ChangeApplicationRoleState>(stateJson));
    }

    public Task SetAsync(ChangeApplicationRoleState state)
    {
        var session = GetSession();
        session.SetString(ChangeApplicationRoleStateKey, JsonSerializer.Serialize(state));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var session = GetSession();
        session.Remove(ChangeApplicationRoleStateKey);
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
