using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Cookies;

/// <summary>
/// Default implementation of the cookie preferences service
/// </summary>
public class CookiePreferencesService : ICookiePreferencesService
{
    private readonly HttpContext _context;
    private readonly CookieSettings _cookieSettings;
    private CookieAcceptanceValues? _pendingAcceptanceValue;

    /// <summary>
    /// Initialises a new instance of the CookiePreferencesService
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    /// <param name="cookieSettings">Cookie settings configuration</param>
    public CookiePreferencesService(
        IHttpContextAccessor httpContextAccessor,
        CookieSettings cookieSettings)
    {
        _context = httpContextAccessor.HttpContext
                    ?? throw new InvalidOperationException("No active HTTP context.");
        _cookieSettings = cookieSettings;
    }

    /// <inheritdoc />
    public void Accept()
    {
        SetCookie(CookieAcceptanceValues.Accept);
    }

    /// <inheritdoc />
    public void Reject()
    {
        SetCookie(CookieAcceptanceValues.Reject);
    }

    /// <inheritdoc />
    public void Reset()
    {
        _context.Response.Cookies.Delete(_cookieSettings.CookieName);
        _pendingAcceptanceValue = CookieAcceptanceValues.Unknown;
    }

    private void SetCookie(CookieAcceptanceValues value)
    {
        _context.Response.Cookies.Append(_cookieSettings.CookieName, value.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(_cookieSettings.ExpiryDays),
            IsEssential = true,
            HttpOnly = true,
            Secure = _context.Request.IsHttps
        });
        _pendingAcceptanceValue = value;
    }

    /// <inheritdoc />
    public CookieAcceptanceValues GetValue()
    {
        if (_pendingAcceptanceValue != null)
        {
            return (CookieAcceptanceValues)_pendingAcceptanceValue;
        }

        if(_context.Request.Cookies.ContainsKey(_cookieSettings.CookieName))
        {
            if (Enum.TryParse(typeof(CookieAcceptanceValues), _context.Request.Cookies[_cookieSettings.CookieName], true, out var result))
            {
                return (CookieAcceptanceValues)result;
            }
        }

        return CookieAcceptanceValues.Unknown;
    }

    /// <inheritdoc />
    public bool IsAccepted()
    {
        return GetValue() == CookieAcceptanceValues.Accept;
    }

    /// <inheritdoc />
    public bool IsRejected()
    {
        return GetValue() == CookieAcceptanceValues.Reject;
    }

    /// <inheritdoc />
    public bool IsUnknown()
    {
        return GetValue() == CookieAcceptanceValues.Unknown;
    }
}
