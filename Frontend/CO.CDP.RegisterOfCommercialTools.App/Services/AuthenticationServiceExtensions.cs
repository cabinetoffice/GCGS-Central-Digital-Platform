using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using CO.CDP.RegisterOfCommercialTools.App.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddOneLoginAuthentication(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        var sessionTimeoutInMinutes = configuration.GetValue<double>("SessionTimeoutInMinutes", 30);
        var cookieSecurePolicy = hostEnvironment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        var oneLoginAuthority = configuration.GetValue<string>("OneLogin:Authority")!;
        var oneLoginClientId = configuration.GetValue<string>("OneLogin:ClientId")!;
        var oneLoginCallback = configuration.GetValue<string>("OneLogin:CallbackPath") ?? "/signin-oidc";
        var oneLoginSignedOutCallback = configuration.GetValue<string>("OneLogin:SignedOutCallbackPath") ?? "/signout-callback-oidc";

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
                options.SlidingExpiration = true;
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = cookieSecurePolicy;

                if (hostEnvironment.IsDevelopment())
                {
                    options.Cookie.Domain = "localhost";
                }
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = oneLoginAuthority;
                options.ClientId = oneLoginClientId;
                options.CallbackPath = oneLoginCallback;
                options.SignedOutCallbackPath = oneLoginSignedOutCallback;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.ResponseMode = OpenIdConnectResponseMode.Query;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("phone");
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.UsePkce = true;
                options.ClientSecret = null;
                options.EventsType = typeof(OidcEvents);
                options.ClaimActions.MapAll();
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = cookieSecurePolicy;
                options.CorrelationCookie.HttpOnly = true;
                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.NonceCookie.SecurePolicy = cookieSecurePolicy;
                options.NonceCookie.HttpOnly = true;
            });

        return services;
    }
}