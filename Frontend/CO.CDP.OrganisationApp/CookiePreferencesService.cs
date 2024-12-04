namespace CO.CDP.OrganisationApp;

public interface ICookiePreferencesService
{
    bool IsAccepted();
    bool IsRejected();
    void Accept();
    void Reject();
    void Reset();
    void SetCookie(CookieAcceptanceValues value);
    bool ValueIs(CookieAcceptanceValues value);
    bool IsUnknown();
}

public class CookiePreferencesService : ICookiePreferencesService
{
    private readonly HttpContext _context;

    private CookieAcceptanceValues pendingAcceptanceValue;

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

    public bool ValueIs(CookieAcceptanceValues value)
    {
        if(pendingAcceptanceValue == value)
        {
            return true;
        }

        return _context.Request.Cookies.ContainsKey(CookieSettings.CookieName)
            && _context.Request.Cookies[CookieSettings.CookieName] == value.ToString();
    }

    public bool IsAccepted()
    {
        return ValueIs(CookieAcceptanceValues.Accept);
    }

    public bool IsRejected()
    {
        return ValueIs(CookieAcceptanceValues.Reject);
    }

    public bool IsUnknown()
    {
        return ValueIs(CookieAcceptanceValues.Unknown);
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