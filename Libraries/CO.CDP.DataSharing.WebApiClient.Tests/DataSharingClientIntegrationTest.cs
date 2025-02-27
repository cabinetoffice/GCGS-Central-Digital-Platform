using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace CO.CDP.DataSharing.WebApiClient.Tests;

public class DataSharingClientIntegrationTest
{
    private readonly HttpClient _httpClient;

    public DataSharingClientIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureServices(s => { s.AddDbContext<OrganisationInformationContext>(o => { o.UseInMemoryDatabase("TestDb"); }); });
            builder.ConfigureLogging(testOutputHelper);
            builder.ConfigureFakePolicyEvaluator();
        });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task ItTalksToTheDataSharingApi()
    {
        var client = new DataSharingClient("https://localhost", _httpClient);

        var shareCode = "HDJ2123F";

        Func<Task> act = async () => { await client.GetSharedDataAsync(shareCode); };

        var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();

        exception.And.StatusCode.Should().Be(404);
        exception.And.Result!.Title.Should().Contain("Not Found");
    }
}