using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.TestKit.Mvc.Tests;

public class TestWebApplicationFactoryTest
{
    [Fact]
    public async void ItEnablesHostConfiguration()
    {
        var factory = new TestWebApplicationFactory<TestProgram>(c =>
        {
            c.UseContentRoot(Directory.GetCurrentDirectory());
            c.ConfigureServices(s =>
            {
                s.AddScoped<Func<string>>(_ => () => "Alice");
            });
        });
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/hello");

        Assert.Equal("Hello, Alice!", await response.Content.ReadAsStringAsync());
    }
}