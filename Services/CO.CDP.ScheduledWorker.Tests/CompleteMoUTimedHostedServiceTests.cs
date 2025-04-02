using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.ScheduledWorker.Tests;

public class CompleteMoUTimedHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider = new();
    private readonly Mock<ILogger<CompleteMoUTimedHostedService>> _mockLogger = new();
    private readonly Mock<IScopedProcessingService> _mockScopedService = new();
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory = new();
    private readonly Mock<IServiceScope> _mockScope = new();
    private readonly CompleteMoUTimedHostedService _service;

    public CompleteMoUTimedHostedServiceTests()
    {
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_mockScopeFactory.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IScopedProcessingService))).Returns(_mockScopedService.Object);
        _service = new CompleteMoUTimedHostedService(_mockServiceProvider.Object,
            new ConfigurationBuilder().AddInMemoryCollection([new("CompleteMoUReminderJob:IntervalInSeconds", "3600")]).Build(),
            _mockLogger.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldStartTimer()
    {
        Func<Task> act = async () => await _service.StartAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Complete MoU Timed Hosted Service running."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldCallScopedService()
    {
        await _service.StartAsync(CancellationToken.None);

        await Task.Delay(100);

        _mockScopedService.Verify(x => x.ExecuteWorkAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldLogError_WhenExceptionOccurs()
    {
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IScopedProcessingService)))
            .Throws(new Exception("Test Exception"));

        await _service.StartAsync(CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred executing ExecuteWorkAsync."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldStopTimer()
    {
        await _service.StopAsync(CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Complete MoU Timed Hosted Service is stopping."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldNotRun_IfPreviousExecutionInProgress()
    {
        _mockScopedService
            .Setup(x => x.ExecuteWorkAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(500); // Simulates long execution time
            });

        // First call starts execution
        var task1 = Task.Run(() =>
            _service.GetType().GetMethod("ExecuteWorkAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_service, [null]));

        await Task.Delay(100); // Ensure first execution starts before second is triggered

        // Second call should be skipped due to semaphore
        var task2 = Task.Run(() =>
            _service.GetType().GetMethod("ExecuteWorkAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_service, [null]));

        await Task.WhenAll(task1, task2);

        _mockScopedService.Verify(x => x.ExecuteWorkAsync(It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Skipping execution - previous work for 'Complete MoU Reminder Service' still in progress"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}