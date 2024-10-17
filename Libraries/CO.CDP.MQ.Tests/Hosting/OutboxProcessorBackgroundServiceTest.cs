using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using CO.CDP.MQ.Tests.Hosting.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Hosting;

public class OutboxProcessorBackgroundServiceTest
{
    private readonly Mock<IOutboxProcessor> _outboxProcessor = new();
    private readonly TestServiceProvider _serviceProvider = new();

    private readonly ILogger<OutboxProcessorBackgroundService> _logger =
        LoggerFactory.Create(_ => { }).CreateLogger<OutboxProcessorBackgroundService>();

    [Fact]
    public async Task ItExecutesTheOutboxProcessor()
    {
        GivenOutboxProcessorIsAvailableInServiceProvider();
        GivenServiceScopeFactoryIsAvailableInServiceContainer();

        var backgroundService = new OutboxProcessorBackgroundService(_serviceProvider,
            new OutboxProcessorBackgroundService.OutboxProcessorConfiguration
            {
                BatchSize = 2,
                ExecutionInterval = TimeSpan.FromSeconds(30)
            },
            _logger);
        await backgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        await backgroundService.StopAsync(CancellationToken.None);

        _outboxProcessor.Verify(d => d.ExecuteAsync(2), Times.Once);
    }

    [Fact]
    public async Task ItContinuesExecutingTheOutboxProcessorInRegularIntervals()
    {
        GivenOutboxProcessorIsAvailableInServiceProvider();
        GivenServiceScopeFactoryIsAvailableInServiceContainer();

        var backgroundService = new OutboxProcessorBackgroundService(_serviceProvider,
            new OutboxProcessorBackgroundService.OutboxProcessorConfiguration
            {
                BatchSize = 3,
                ExecutionInterval = TimeSpan.FromMilliseconds(4)
            },
            _logger);
        await backgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        await backgroundService.StopAsync(CancellationToken.None);

        _outboxProcessor.Verify(d => d.ExecuteAsync(3), Times.AtLeast(2));
    }

    private void GivenServiceScopeFactoryIsAvailableInServiceContainer()
    {
        _serviceProvider.Services.Add(typeof(IServiceScopeFactory), new TestServiceScopeFactory(_serviceProvider));
    }

    private void GivenOutboxProcessorIsAvailableInServiceProvider()
    {
        _serviceProvider.Services.Add(typeof(IOutboxProcessor), _outboxProcessor.Object);
    }
}