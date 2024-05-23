using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.TestKit.Mvc;

public class TestWebApplicationFactory<TProgram>(Action<IHostBuilder> configurator)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        configurator(builder);

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication("FakeBearer")
                      .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("FakeBearer", o => { });
        });

        return base.CreateHost(builder);
    }
}