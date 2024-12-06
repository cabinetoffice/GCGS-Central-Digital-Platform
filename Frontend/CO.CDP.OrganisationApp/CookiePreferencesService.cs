namespace CO.CDP.OrganisationApp;

public class CookiePreferencesService : ICookiePreferencesService
{
    private readonly HttpContext _context;

    private CookieAcceptanceValues? pendingAcceptanceValue;

    public CookiePreferencesService(IHttpContextAccessor httpContextAccessor)
    {
        _context = httpContextAccessor.HttpContext
                    ?? throw new InvalidOperationException("No active HTTP context.");
    }

    public void Accept()
    {
        SetCookie(CookieAcceptanceValues.Accept);
    }

    public void Reject()
    {
        SetCookie(CookieAcceptanceValues.Reject);        
    }

    public void Reset()
    {
        _context.Response.Cookies.Delete(CookieSettings.CookieName);
        pendingAcceptanceValue = CookieAcceptanceValues.Unknown;
    }

    public void SetCookie(CookieAcceptanceValues value)
    {
        _context.Response.Cookies.Append(CookieSettings.CookieName, value.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(365),
            IsEssential = true,
            HttpOnly = true,
            Secure = _context.Request.IsHttps
        });
        pendingAcceptanceValue = value;
    }

    public CookieAcceptanceValues GetValue()
    {
        if (pendingAcceptanceValue != null)
        {
            return (CookieAcceptanceValues)pendingAcceptanceValue;
        }

        if(_context.Request.Cookies.ContainsKey(CookieSettings.CookieName))
        {
            if (Enum.TryParse(typeof(CookieAcceptanceValues), _context.Request.Cookies[CookieSettings.CookieName], true, out var result))
            {
                return (CookieAcceptanceValues)result;
            }
        }

        return CookieAcceptanceValues.Unknown;

    }

    public bool IsAccepted()
    {
        return GetValue() == CookieAcceptanceValues.Accept;
    }

    public bool IsRejected()
    {
        return GetValue() == CookieAcceptanceValues.Reject;
    }

    public bool IsUnknown()
    {
        return GetValue() == CookieAcceptanceValues.Unknown;
    }
}
public enum CookieAcceptanceValues
{
    Unknown,
    Accept,
    Reject
}

public static class CookieSettings
{
    public const string CookieAcceptanceFieldName = "CookieAcceptance";
    public const string CookieSettingsPageReturnUrlFieldName = "ReturnUrl";
    public const string CookieBannerInteractionQueryString = "cookieBannerInteraction";
    public const string CookieName = "SIRSI_COOKIES_PREFERENCES_SET";
    public const string FtsHandoverParameter = "cookies_accepted";
}