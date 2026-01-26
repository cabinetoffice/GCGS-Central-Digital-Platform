using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Services;

public class AppSessionService(IHttpContextAccessor httpContextAccessor) : IAppSession
{
    public T? Get<T>(string key)
    {
        var value = GetSession().GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public void Set<T>(string key, T value)
    {
        GetSession().SetString(key, JsonSerializer.Serialize(value));
    }

    public void Remove(string key)
    {
        GetSession().Remove(key);
    }

    public void Clear()
    {
        GetSession().Clear();
    }

    private ISession GetSession()
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
            throw new InvalidOperationException("Session is not available. Ensure session middleware is configured.");
        return session;
    }
}
