using CO.CDP.Authentication;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace CO.CDP.DataSharing.WebApiClient.Tests;

public class DataSharingClientIntegrationTest
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IClaimService> _claimService = new();

    private readonly HttpClient _httpClient;

    public DataSharingClientIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
            builder.ConfigureLogging(testOutputHelper);
            builder.ConfigureServices((_, s) =>
            {
                s.AddScoped(_ => _shareCodeRepository.Object);
                s.AddScoped(_ => _claimService.Object);
            });
        });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task ItTalksToTheDataSharingApi()
    {
        IDataSharingClient client = new DataSharingClient("https://localhost", _httpClient);

        var shareCode = "HDJ2123F";
        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, shareCode)).ReturnsAsync(true);

        Func<Task> act = async () => { await client.GetSharedDataAsync(shareCode); };

        var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();

        exception.And.StatusCode.Should().Be(404);
        exception.And.Result!.Title.Should().Contain("ShareCodeNotFoundException");
    }
}