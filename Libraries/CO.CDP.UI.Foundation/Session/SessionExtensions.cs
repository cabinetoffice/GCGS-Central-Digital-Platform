using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CO.CDP.UI.Foundation.Session;

/// <summary>
/// Extension methods for ISession
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Gets a typed value from the session
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve</typeparam>
    /// <param name="session">The session</param>
    /// <param name="key">The session key</param>
    /// <returns>The deserialised value or default if not found</returns>
    public static T? Get<T>(this ISession session, string key)
    {
        var data = session.GetString(key);
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a typed value in the session
    /// </summary>
    /// <typeparam name="T">The type of the value to store</typeparam>
    /// <param name="session">The session</param>
    /// <param name="key">The session key</param>
    /// <param name="value">The value to store</param>
    public static void Set<T>(this ISession session, string key, T value)
    {
        var data = JsonSerializer.Serialize(value);
        session.SetString(key, data);
    }
}