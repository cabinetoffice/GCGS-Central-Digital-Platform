using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationOverviewTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly OrganisationOverviewModel _model;

    public OrganisationOverviewTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new OrganisationOverviewModel(_organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_WithValidId_CallsGetOrganisationAsync()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await _model.OnGet(id);

        _organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, id!.Value, null, "Test Org", []);
    }
}