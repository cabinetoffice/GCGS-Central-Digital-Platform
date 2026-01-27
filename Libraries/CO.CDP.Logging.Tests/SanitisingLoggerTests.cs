using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CO.CDP.Logging.Tests;

public class SanitisingLoggerTests
{
    [Fact]
    public void SanitisesNewlines_InLogMessage()
    {
        var logMessages = new List<string>();
        
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider(logMessages)));
        services.AddSanitisedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<SanitisingLoggerTests>>();
        
        var maliciousInput = "admin\n[CRITICAL] System hacked!\nadmin";
        
        logger.LogInformation("User: {User}", maliciousInput);
        
        logMessages.Should().HaveCount(1);
        logMessages[0].Should().NotContain("\n");
        logMessages[0].Should().Contain("admin [CRITICAL] System hacked! admin");
    }

    [Fact]
    public void SanitisesCarriageReturns_InLogMessage()
    {
        var logMessages = new List<string>();
        
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider(logMessages)));
        services.AddSanitisedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<SanitisingLoggerTests>>();
        
        var maliciousInput = "test\r\ninjection\rattack";
        
        logger.LogWarning("Processing: {Input}", maliciousInput);
        
        logMessages.Should().HaveCount(1);
        var actualMessage = logMessages[0];
        actualMessage.Should().NotContain("\r");
        actualMessage.Should().NotContain("\n");
        actualMessage.Should().Contain("test");
        actualMessage.Should().Contain("injection");
        actualMessage.Should().Contain("attack");
    }

    [Fact]
    public void SanistisesTabs_InLogMessage()
    {
        var logMessages = new List<string>();
        
        var services = new ServiceCollection();
        services.AddLogging(builder => 
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new TestLoggerProvider(logMessages));
        });
        services.AddSanitisedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<SanitisingLoggerTests>>();
        
        var maliciousInput = "data\twith\ttabs";
        
        logger.LogDebug("Data: {Data}", maliciousInput);
        
        logMessages.Should().HaveCount(1);
        logMessages[0].Should().NotContain("\t");
        logMessages[0].Should().Contain("data with tabs");
    }

    [Fact]
    public void ReturnsCorrectLogger_FromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSanitisedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<SanitisingLoggerTests>>();
        
        logger.Should().NotBeNull();
        logger.Should().BeOfType<SanitisingLogger<SanitisingLoggerTests>>();
    }

    [Fact]
    public void PreservesLogLevel_WhenSanitising()
    {
        var logMessages = new List<string>();
        
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new TestLoggerProvider(logMessages));
        });
        services.AddSanitisedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<SanitisingLoggerTests>>();
        
        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");
        
        logMessages.Should().HaveCount(1);
        logMessages[0].Should().Contain("Info message");
    }
}

public class TestLoggerProvider(List<string> logMessages) : ILoggerProvider
{
    private bool _disposed;

    public ILogger CreateLogger(string categoryName) => new TestLogger(logMessages);
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
    }
}

public class TestLogger(List<string> logMessages) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter)
    {
        logMessages.Add(formatter(state, exception));
    }
}
