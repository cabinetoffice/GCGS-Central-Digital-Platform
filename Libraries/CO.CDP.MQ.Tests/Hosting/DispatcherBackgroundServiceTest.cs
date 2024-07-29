using CO.CDP.MQ.Hosting;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.MQ.Tests.Hosting;

public class DispatcherBackgroundServiceTest
{
    private readonly Mock<IDispatcher> _dispatcher = new();
    private readonly TestServiceProvider _serviceProvider = new();

    [Fact]
    public void ItExecutesTheQueueDispatcher()
    {
        _serviceProvider.Services.Add(typeof(IDispatcher), _dispatcher.Object);
        _serviceProvider.Services.Add(typeof(IServiceScopeFactory), new TestServiceScopeFactory(_serviceProvider));

        var backgroundService = new DispatcherBackgroundService(_serviceProvider);
        var cancellationToken = CancellationToken.None;
        var task = Task.CompletedTask;
        _dispatcher.Setup(d => d.ExecuteAsync(cancellationToken))
            .Returns(task);

        var result = backgroundService.StartAsync(cancellationToken);

        result.Should().Be(task);
    }
}

internal class TestServiceScopeFactory(IServiceProvider services) : IServiceScopeFactory
{
    public IServiceScope CreateScope()
    {
        return new TestServiceScope(services);
    }
}

internal class TestServiceScope(IServiceProvider services) : IServiceScope
{
    public void Dispose()
    {
    }

    public IServiceProvider ServiceProvider => services;
}

internal class TestServiceProvider : IServiceProvider
{
    public readonly Dictionary<Type, object> Services = new();

    public object? GetService(Type serviceType)
    {
        return Services.GetValueOrDefault(serviceType);
    }
}