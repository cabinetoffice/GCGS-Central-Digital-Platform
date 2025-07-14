namespace CO.CDP.UI.Foundation.Cookies;

/// <summary>
/// Interface for managing user cookie preferences across applications
/// </summary>
public interface ICookiePreferencesService
{
    /// <summary>
    /// Checks if the user has accepted cookies
    /// </summary>
    bool IsAccepted();

    /// <summary>
    /// Checks if the user has rejected cookies
    /// </summary>
    bool IsRejected();

    /// <summary>
    /// Sets the user's preference to accept cookies
    /// </summary>
    void Accept();

    /// <summary>
    /// Sets the user's preference to reject cookies
    /// </summary>
    void Reject();

    /// <summary>
    /// Resets the user's cookie preferences
    /// </summary>
    void Reset();

    /// <summary>
    /// Checks if the user has not set a cookie preference
    /// </summary>
    bool IsUnknown();

    /// <summary>
    /// Gets the current cookie acceptance value
    /// </summary>
    CookieAcceptanceValues GetValue();
}

/// <summary>
/// Enum representing possible cookie acceptance states
/// </summary>
public enum CookieAcceptanceValues
{
    /// <summary>
    /// User has not made a choice
    /// </summary>
    Unknown,

    /// <summary>
    /// User has accepted cookies
    /// </summary>
    Accept,

    /// <summary>
    /// User has rejected cookies
    /// </summary>
    Reject
}
