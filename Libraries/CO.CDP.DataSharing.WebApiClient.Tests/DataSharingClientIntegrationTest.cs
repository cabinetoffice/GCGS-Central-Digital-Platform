using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Xunit.Abstractions;

namespace CO.CDP.DataSharing.WebApiClient.Tests;

public class DataSharingClientIntegrationTest(ITestOutputHelper testOutputHelper)
{
    private readonly TestWebApplicationFactory<Program> _factory = new(builder =>
    {
        builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
        builder.ConfigureLogging(testOutputHelper);
    });

    [Fact]
    public async Task ItTalksToTheDataSharingApi()
    {
        IDataSharingClient client = new DataSharingClient("https://localhost", _factory.CreateClient());

        Func<Task> act = async () => { await client.GetSharedDataAsync("HDJ2123F"); };

        var exception = await act.Should().NotThrowAsync<ApiException<ProblemDetails>>();

        exception.Should().NotBe(404);
        //TODO: Make this test consistent with OrganisationClientIntegrationTest & PersonClientIntegrationTest as part of DP-319
    }
}