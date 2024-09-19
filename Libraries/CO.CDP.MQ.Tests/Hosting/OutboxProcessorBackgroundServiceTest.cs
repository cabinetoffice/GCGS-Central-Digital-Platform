using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using CO.CDP.MQ.Tests.Hosting.TestKit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.MQ.Tests.Hosting;

public class OutboxProcessorBackgroundServiceTest
{
    private readonly Mock<IOutboxProcessor> _outboxProcessor = new();
    private readonly TestServiceProvider _serviceProvider = new();

    [Fact]
    public void ItExecutesTheOutboxProcessor()
    {
        GivenOutboxProcessorIsAvailableInServiceProvider();
        GivenServiceScopeFactoryIsAvailableInServiceContainer();

        var task = Task.CompletedTask;
        _outboxProcessor.Setup(d => d.ExecuteAsync(2))
            .Returns(task);

        var backgroundService = new OutboxProcessorBackgroundService(_serviceProvider,
            new OutboxProcessorBackgroundService.OutboxProcessorConfiguration
            {
                BatchSize = 2
            });
        var result = backgroundService.StartAsync(CancellationToken.None);

        result.Should().Be(task);
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