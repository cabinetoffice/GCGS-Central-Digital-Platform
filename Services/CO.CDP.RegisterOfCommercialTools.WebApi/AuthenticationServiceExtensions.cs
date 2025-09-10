using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace CO.CDP.RegisterOfCommercialTools.WebApi;

public static class AuthenticationServiceExtensions
{
    public static void AddPassThroughAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("PassThrough")
            .AddScheme<AuthenticationSchemeOptions, PassThroughAuthHandler>("PassThrough", _ => { });

        services.AddAuthorization();
    }
}

public class PassThroughAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}