using Serilog.Events;

namespace CO.CDP.AwsServices;

public record LoggingConfiguration
{
    public MinimumLevel MinimumLevel { get; init; } = new();
}

public record MinimumLevel
{
    public LogEventLevel Default { get; init; } = LogEventLevel.Information;

    public Dictionary<string, LogEventLevel> Override { get; init; } = new();
}