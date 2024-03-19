using CO.CDP.Tenant.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.Tenant.WebApi.Tests.Fixtures;

public class TestWebApplicationFactory<TProgram>(ITenantRepository tenantRepository)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.AddScoped<ITenantRepository>(_ => tenantRepository); });
        return base.CreateHost(builder);
    }
}