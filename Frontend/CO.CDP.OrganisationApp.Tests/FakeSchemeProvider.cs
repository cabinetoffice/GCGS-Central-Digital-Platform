using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Tests;

public class FakeSchemeProvider : AuthenticationSchemeProvider
{
    public FakeSchemeProvider(IOptions<AuthenticationOptions> options)
        : base(options)
    {
    }

    protected FakeSchemeProvider(
        IOptions<AuthenticationOptions> options,
        IDictionary<string, AuthenticationScheme> schemes
    )
        : base(options, schemes)
    {
    }

    public override Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        if (name == FakeCookieAuthHandler.AuthenticationScheme)
        {
            return Task.FromResult<AuthenticationScheme?>(new AuthenticationScheme(
                FakeCookieAuthHandler.AuthenticationScheme,
                FakeCookieAuthHandler.AuthenticationScheme,
                typeof(FakeCookieAuthHandler)
            ));
        }

        return base.GetSchemeAsync(name);
    }
}