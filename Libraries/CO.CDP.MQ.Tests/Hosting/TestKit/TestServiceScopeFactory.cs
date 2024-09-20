using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.MQ.Tests.Hosting.TestKit;

internal class TestServiceScopeFactory(IServiceProvider services) : IServiceScopeFactory
{
    public IServiceScope CreateScope()
    {
        return new TestServiceScope(services);
    }
}