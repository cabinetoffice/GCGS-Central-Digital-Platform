using System.Net;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace CO.CDP.TestKit.Mvc.Tests;

internal class TestOutput : ITestOutputHelper
{
    public readonly List<string> Messages = [];

    public void WriteLine(string message)
    {
        Messages.Add(message);
    }

    public void WriteLine(string format, params object[] args)
    {
    }
}

public class LoggingExtensionsTest
{
    private readonly TestOutput _testOutput = new TestOutput();

    [Fact]
    public async void ItConfiguresLoggingProvider()
    {
        var factory = new TestWebApplicationFactory<TestProgram>(c =>
        {
            c.UseContentRoot(Directory.GetCurrentDirectory());
            c.ConfigureLogging(s => s.AddProvider(_testOutput));
        });
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/log");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Hello!", _testOutput.Messages);
    }

    [Fact]
    public async void ItConfiguresLogging()
    {
        var factory = new TestWebApplicationFactory<TestProgram>(c =>
        {
            c.UseContentRoot(Directory.GetCurrentDirectory());
            c.ConfigureLogging(_testOutput);
        });
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/log");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Hello!", _testOutput.Messages);
    }
}