using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CO.CDP.TestKit.Mvc;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddProvider<TO>(this ILoggingBuilder logging, TO testOutputHelper)
        where TO : ITestOutputHelper => logging.AddProvider(new XUnitLoggerProvider(testOutputHelper));
}