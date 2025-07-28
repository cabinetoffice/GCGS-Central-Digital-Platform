using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAwsCognitoAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        var sessionTimeoutInMinutes = configuration.GetValue<double>("SessionTimeoutInMinutes", 30);
        var cookieSecurePolicy = hostEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "CommercialTools.Auth";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSecurePolicy;
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/AccessDenied";

            if (hostEnvironment.IsDevelopment())
            {
                options.Cookie.Domain = "localhost";
            }
        });

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect(SetupOpenIdConnect(configuration, hostEnvironment));

        return services;
    }

    private static Action<OpenIdConnectOptions> SetupOpenIdConnect(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        return options =>
        {
            var userPoolId = configuration.GetValue<string>("AWS:CognitoAuthentication:UserPoolId");
            var clientId = configuration.GetValue<string>("AWS:CognitoAuthentication:UserPoolClientId");
            var clientSecret = configuration.GetValue<string>("AWS:CognitoAuthentication:UserPoolClientSecret");
            var domain = configuration.GetValue<string>("AWS:CognitoAuthentication:Domain");
            var region = configuration.GetValue<string>("AWS:Region");

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.MetadataAddress =
                $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/openid-configuration";

            options.ClientId = clientId;
            options.ClientSecret = clientSecret;

            options.Scope.Clear();
            options.Scope.Add("email");
            options.Scope.Add("openid");
            options.Scope.Add("phone");

            options.SaveTokens = true;

            options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment();

            options.Events = new OpenIdConnectEvents()
            {
                OnRedirectToIdentityProvider = context =>
                {
                    var request = context.HttpContext.Request;
                    var host = request.Host.Value;
                    var callbackPath = "/signin-oidc";

                    string scheme = host.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase) ? "http" : "https";

                    var redirectUri = $"{scheme}://{host}{callbackPath}";
                    context.ProtocolMessage.RedirectUri = redirectUri;

                    return Task.CompletedTask;
                },

                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    var request = context.HttpContext.Request;
                    var host = request.Host.Value;

                    string scheme = host.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase) ? "http" : "https";

                    var signedOutUrl = $"{scheme}://{host}/signout-callback-oidc";

                    context.ProtocolMessage.IssuerAddress =
                        $"https://{domain}/logout?client_id={clientId}&logout_uri={Uri.EscapeDataString(signedOutUrl)}";

                    return Task.CompletedTask;
                }
            };
        };
    }
}