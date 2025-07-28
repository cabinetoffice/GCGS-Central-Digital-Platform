using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages.Auth;

public class LoginModel(ISession session, ILogger<LoginModel> logger, IConfiguration configuration) : PageModel
{
    public IActionResult OnGet(string? returnUrl = null, string? origin = null)
    {
        SetServiceOrigins(returnUrl, origin);

        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        };

        return Challenge(properties);
    }

    private void SetServiceOrigins(string? redirectUri, string? origin)
    {
        session.Remove(Session.FtsServiceOrigin);
        session.Remove(Session.SirsiServiceOrigin);

        try
        {
            if (!string.IsNullOrWhiteSpace(origin))
            {
                ValidateAndSetOrigin(origin);
            }

            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                var uri = new Uri("https://example.com" + redirectUri);
                var queryOrigin = HttpUtility.ParseQueryString(uri.Query).Get("origin");
                if (!string.IsNullOrWhiteSpace(queryOrigin))
                {
                    ValidateAndSetOrigin(queryOrigin);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Invalid redirectUri or origin, {Exception}", ex);
        }
    }

    private void ValidateAndSetOrigin(string origin)
    {
        // Get allowed origins from configuration
        var ftsAllowedOrigins = configuration["FtsServiceAllowedOrigins"] ?? "";
        var sirsiAllowedOrigins = configuration["SirsiServiceAllowedOrigins"] ?? "";

        var ftsOrigins = ftsAllowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries);
        var sirsiOrigins = sirsiAllowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries);

        if (ftsOrigins.Contains(origin))
        {
            session.Set(Session.FtsServiceOrigin, origin);
        }
        else if (sirsiOrigins.Contains(origin))
        {
            session.Set(Session.SirsiServiceOrigin, origin);
        }
        else
        {
            logger.LogWarning("Origin {Origin} is not in allowed origins list", origin);
        }
    }
}