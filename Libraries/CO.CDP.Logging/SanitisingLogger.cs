using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CO.CDP.Logging;

/// <summary>
/// Logger wrapper that automatically sanitises log output to prevent log injection attacks.
/// Replaces newlines and control characters with spaces.
/// </summary>
public class SanitisingLogger<T>(ILoggerFactory factory) : ILogger<T>
{
    private readonly ILogger _logger = factory.CreateLogger(typeof(T).FullName ?? typeof(T).Name);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        _logger.Log(logLevel, eventId, state, exception, (s, e) => Sanitise(formatter(s, e)));
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state);

    private static string Sanitise(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;

        return message
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ");
    }
}

public static class SanitisingLoggerExtensions
{
    /// <summary>
    /// Adds sanitising logger that automatically prevents log injection attacks.
    /// Call this after AddLogging() to wrap the default logger.
    /// </summary>
    public static IServiceCollection AddSanitisedLogging(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Transient(typeof(ILogger<>), typeof(SanitisingLogger<>)));
        return services;
    }
}
