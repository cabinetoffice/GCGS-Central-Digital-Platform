using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.MQ.Tests.Hosting.TestKit;

internal class TestServiceScope(IServiceProvider services) : IServiceScope
{
    public void Dispose()
    {
    }

    public IServiceProvider ServiceProvider => services;
}