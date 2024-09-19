using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using CO.CDP.MQ.Tests.Hosting.TestKit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Range = Moq.Range;

namespace CO.CDP.MQ.Tests.Hosting;

public class OutboxProcessorBackgroundServiceTest
{
    private readonly Mock<IOutboxProcessor> _outboxProcessor = new();
    private readonly TestServiceProvider _serviceProvider = new();

    [Fact]
    public async Task ItExecutesTheOutboxProcessor()
    {
        GivenOutboxProcessorIsAvailableInServiceProvider();
        GivenServiceScopeFactoryIsAvailableInServiceContainer();

        var backgroundService = new OutboxProcessorBackgroundService(_serviceProvider,
            new OutboxProcessorBackgroundService.OutboxProcessorConfiguration
            {
                BatchSize = 2
            });
        await backgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(1);
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
            });
        await backgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(6));
        await backgroundService.StopAsync(CancellationToken.None);

        _outboxProcessor.Verify(d => d.ExecuteAsync(It.IsInRange(2, 3, Range.Inclusive)), Times.Exactly(2));
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