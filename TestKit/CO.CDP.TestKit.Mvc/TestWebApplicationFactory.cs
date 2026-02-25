using CO.CDP.MQ;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CO.CDP.TestKit.Mvc;

public class TestWebApplicationFactory<TProgram>(Action<IHostBuilder> configurator)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        configurator(builder);
        builder.ConfigureServices(services =>
        {
            var mockPublisher = new Mock<IPublisher>();
            services.AddScoped(_ => mockPublisher.Object);
        });

        return base.CreateHost(builder);
    }
}
