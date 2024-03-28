using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.Tenant.WebApi.Tests.Api.WebApp;

public class TestWebApplicationFactory<TProgram>(Action<IHostBuilder> configurator)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        configurator(builder);
        return base.CreateHost(builder);
    }
}