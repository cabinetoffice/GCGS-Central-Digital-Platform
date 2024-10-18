using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class JoinOrganisationSuccessModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly JoinOrganisationSuccessModel _joinOrganisationSuccessModel;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();
    private readonly CO.CDP.Organisation.WebApiClient.Organisation _organisation;

    public JoinOrganisationSuccessModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _joinOrganisationSuccessModel = new JoinOrganisationSuccessModel(_organisationClientMock.Object);
        _organisation = new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, _organisationId, null, "Test Org", []);
    }

    [Fact]
    public async Task OnGet_ValidOrganisationId_ReturnsPageResult()
    {
        var organisationId = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(organisationId))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationSuccessModel.OnGet(organisationId);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationSuccessModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.GetOrganisationAsync(organisationId), Times.Once);
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _joinOrganisationSuccessModel.OnGet(_organisationId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");

        _organisationClientMock.Verify(client => client.GetOrganisationAsync(_organisationId), Times.Once);
    }
}
