using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.Tenant.WebApi.Tests.Api.WebApp;

public class TestWebApplicationFactory<TProgram>(Action<IServiceCollection> configurator)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(configurator);
        return base.CreateHost(builder);
    }
}