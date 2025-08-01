using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Session;

/// <summary>
/// Interface for session management operations
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Gets a value from the session
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve</typeparam>
    /// <param name="key">The session key</param>
    /// <returns>The session value or default if not found</returns>
    T? GetValue<T>(string key);

    /// <summary>
    /// Sets a value in the session
    /// </summary>
    /// <typeparam name="T">The type of value to set</typeparam>
    /// <param name="key">The session key</param>
    /// <param name="value">The value to store</param>
    void SetValue<T>(string key, T value);

    /// <summary>
    /// Removes a value from the session
    /// </summary>
    /// <param name="key">The session key</param>
    void RemoveValue(string key);

    /// <summary>
    /// Checks if a key exists in the session
    /// </summary>
    /// <param name="key">The session key</param>
    /// <returns>True if the key exists, otherwise false</returns>
    bool HasKey(string key);

    /// <summary>
    /// Gets a string value from the session
    /// </summary>
    /// <param name="key">The session key</param>
    /// <returns>The string value or null if not found</returns>
    string? GetString(string key);

    /// <summary>
    /// Sets a string value in the session
    /// </summary>
    /// <param name="key">The session key</param>
    /// <param name="value">The string value to store</param>
    void SetString(string key, string value);
}

/// <summary>
/// Implementation of session management service
/// </summary>
public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the SessionService
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor for session access</param>
    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public T? GetValue<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        return session.Get<T>(key);
    }

    /// <inheritdoc />
    public void SetValue<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        session.Set(key, value);
    }

    /// <inheritdoc />
    public void RemoveValue(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        session.Remove(key);
    }

    /// <inheritdoc />
    public bool HasKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        return session.Keys.Contains(key);
    }

    /// <inheritdoc />
    public string? GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        return session.GetString(key);
    }

    /// <inheritdoc />
    public void SetString(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));

        var session = GetSession();
        session.SetString(key, value);
    }

    private ISession GetSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session
                      ?? throw new InvalidOperationException("HTTP context or session is not available");

        return session;
    }
}