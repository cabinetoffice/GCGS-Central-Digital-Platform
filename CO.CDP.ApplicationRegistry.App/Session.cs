using System.Text.Json;

namespace CO.CDP.ApplicationRegistry.App;

public class Session(IHttpContextAccessor httpContextAccessor) : IAppSession
{
    public const string UserDetailsKey = "UserDetails";
    public const string UserAuthTokens = "UserAuthTokens";

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

    private ISession GetSession()
    {
        try
        {
            return httpContextAccessor.HttpContext?.Session == null ? throw new InvalidOperationException("Session is not available") : httpContextAccessor.HttpContext!.Session;
        }
        catch (InvalidOperationException cause)
        {
            throw new InvalidOperationException("Session is not available", cause);
        }
    }

    public void Clear()
    {
        GetSession().Clear();
    }
}

