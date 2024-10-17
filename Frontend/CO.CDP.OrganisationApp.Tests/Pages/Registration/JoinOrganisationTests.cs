using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class JoinOrganisationModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly JoinOrganisationModel _joinOrganisationModel;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();
    private readonly CDP.Organisation.WebApiClient.Organisation _organisation;

    public JoinOrganisationModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = _personId});
        _joinOrganisationModel = new JoinOrganisationModel(_organisationClientMock.Object, _sessionMock.Object);
        _organisation = new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, _organisationId, null, "Test Org", []);
    }

    [Fact]
    public async Task OnGet_ValidOrganisationId_ReturnsPageResult()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationModel.OnGet(_organisationId);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.GetOrganisationAsync(_organisationId), Times.Once);
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _joinOrganisationModel.OnGet(_organisationId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");

        _organisationClientMock.Verify(client => client.GetOrganisationAsync(_organisationId), Times.Once);
    }

    [Fact]
    public async Task OnPost_ModelStateInvalid_ReturnsPageResultWithOrganisationDetails()
    {
        _joinOrganisationModel.ModelState.AddModelError("Join", "Select an option");

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationModel.OnPost(_organisationId);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.GetOrganisationAsync(_organisationId), Times.Once);
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsTrue_CreatesJoinRequestAndRedirectsToSuccessPage()
    {
        var personId = Guid.NewGuid();
        _joinOrganisationModel.Join = true;

        var result = await _joinOrganisationModel.OnPost(_organisationId);

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(
            _organisationId,
            It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)),
            Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_organisationId}/join-organisation/success");
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsFalse_RedirectsToCompaniesHouseNumber()
    {
        var organisationId = Guid.NewGuid();
        _joinOrganisationModel.Join = false;

        var result = await _joinOrganisationModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/registration/has-companies-house-number");

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<CreateOrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_UserHasNoPersonId_RedirectsToHomePage()
    {
        var organisationId = Guid.NewGuid();
        _joinOrganisationModel.Join = true;
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = null});

        var result = await _joinOrganisationModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/");

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<CreateOrganisationJoinRequest>()), Times.Never);
    }
}
