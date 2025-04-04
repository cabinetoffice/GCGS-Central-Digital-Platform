using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Tests.Hosting.TestKit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.MQ.Tests.Hosting;

public class DispatcherBackgroundServiceTest
{
    private readonly Mock<IDispatcher> _dispatcher = new();
    private readonly TestServiceProvider _serviceProvider = new();

    [Fact]
    public async Task ItExecutesTheQueueDispatcher()
    {
        _serviceProvider.Services.Add(typeof(IDispatcher), _dispatcher.Object);
        _serviceProvider.Services.Add(typeof(IServiceScopeFactory), new TestServiceScopeFactory(_serviceProvider));

        var backgroundService = new DispatcherBackgroundService(_serviceProvider);

        await backgroundService.StartAsync(CancellationToken.None);

        _dispatcher.Verify(d => d.ExecuteAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}