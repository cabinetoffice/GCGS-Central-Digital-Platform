using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.TestKit.Mvc.Tests;

public class EntityFrameworkServiceCollectionExtensionsTest
{
    [Fact]
    public async void ItConfiguresInMemoryDbContextService()
    {
        var factory = new TestWebApplicationFactory<TestProgram>(c =>
        {
            c.UseContentRoot(Directory.GetCurrentDirectory());
            c.ConfigureServices(s => s.ConfigureInMemoryDbContext<DbContext>());
        });
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/db");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async void ItConfiguresInMemoryDbContext()
    {
        var factory = new TestWebApplicationFactory<TestProgram>(c =>
        {
            c.UseContentRoot(Directory.GetCurrentDirectory());
            c.ConfigureInMemoryDbContext<DbContext>();
        });
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/db");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", await response.Content.ReadAsStringAsync());
    }
}