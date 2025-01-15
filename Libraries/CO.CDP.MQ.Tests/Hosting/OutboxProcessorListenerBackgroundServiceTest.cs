using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using CO.CDP.MQ.Tests.Hosting.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Hosting;

public class OutboxProcessorListenerBackgroundServiceTest
{
    private readonly Mock<IOutboxProcessorListener> _outboxProcessorListener = new();
    private readonly TestServiceProvider _serviceProvider = new();
    private readonly CancellationTokenSource _tokenSource = new();
    private readonly ILogger<OutboxProcessorListenerBackgroundService> _logger =
        LoggerFactory.Create(_ => { }).CreateLogger<OutboxProcessorListenerBackgroundService>();

    [Fact]
    public async Task ItStartsTheOutboxProcessorListener()
    {
        GivenOutboxProcessorListenerIsAvailableInServiceProvider();
        GivenServiceScopeFactoryIsAvailableInServiceContainer();

        var backgroundService =
            new OutboxProcessorListenerBackgroundService(_serviceProvider, ConfigurationFor(batchSize: 2), _logger);
        await backgroundService.StartAsync(_tokenSource.Token);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        await backgroundService.StopAsync(_tokenSource.Token);

        _outboxProcessorListener
            .Verify(d => d.WaitAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static OutboxProcessorBackgroundService.OutboxProcessorConfiguration ConfigurationFor(int batchSize = 0)
    {
        return new OutboxProcessorBackgroundService.OutboxProcessorConfiguration
        {
            BatchSize = batchSize,
            ExecutionInterval = TimeSpan.FromSeconds(30)
        };
    }

    private void GivenOutboxProcessorListenerIsAvailableInServiceProvider()
    {
        _serviceProvider.Services.Add(typeof(IOutboxProcessorListener), _outboxProcessorListener.Object);
    }

    private void GivenServiceScopeFactoryIsAvailableInServiceContainer()
    {
        _serviceProvider.Services.Add(typeof(IServiceScopeFactory), new TestServiceScopeFactory(_serviceProvider));
    }
}