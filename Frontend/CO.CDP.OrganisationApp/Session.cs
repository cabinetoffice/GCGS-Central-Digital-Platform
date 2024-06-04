using System.Text.Json;

namespace CO.CDP.OrganisationApp;

public class Session(IHttpContextAccessor httpContextAccessor) : ISession
{
    public const string UserDetailsKey = "UserDetails";
    public const string RegistrationDetailsKey = "RegistrationDetails";

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

    private Microsoft.AspNetCore.Http.ISession GetSession()
    {
        try
        {
            if (httpContextAccessor.HttpContext?.Session == null)
            {
                throw new Exception("Session is not available");
            }
            return httpContextAccessor.HttpContext!.Session;
        }
        catch (InvalidOperationException cause)
        {
            throw new Exception("Session is not available", cause);
        }
    }

    public void Clear()
    {
        GetSession().Clear();
    }
}