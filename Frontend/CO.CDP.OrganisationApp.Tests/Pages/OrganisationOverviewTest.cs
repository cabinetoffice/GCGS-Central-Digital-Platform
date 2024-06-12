using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationOverviewTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<ISession> sessionMock;

    public OrganisationOverviewTest()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public async Task OnGet_WithValidId_CallsGetOrganisationAsync()
    {
        var id = Guid.NewGuid();
        var model = GivenOrganisationOverviewModel();
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await model.OnGet(id);

        organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        var model = GivenOrganisationOverviewModel();

        organisationClientMock.Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, id!.Value, null, "Test Org", []);
    }

    private OrganisationOverviewModel GivenOrganisationOverviewModel()
    {
        return new OrganisationOverviewModel(organisationClientMock.Object, sessionMock.Object);
    }
}