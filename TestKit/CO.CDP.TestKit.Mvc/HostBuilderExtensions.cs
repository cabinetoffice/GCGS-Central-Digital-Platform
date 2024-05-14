using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace CO.CDP.TestKit.Mvc;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureInMemoryDbContext<TC>(this IHostBuilder builder) where TC : DbContext =>
        builder.ConfigureServices(services => services.ConfigureInMemoryDbContext<TC>());

    public static IHostBuilder ConfigureLogging<TO>(this IHostBuilder builder, TO testOutputHelper) where TO : ITestOutputHelper =>
        builder.ConfigureLogging(logging => logging.AddProvider(testOutputHelper));
}