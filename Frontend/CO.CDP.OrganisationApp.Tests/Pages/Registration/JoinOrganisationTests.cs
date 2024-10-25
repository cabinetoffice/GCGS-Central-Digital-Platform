using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class JoinOrganisationModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<ITempDataService> _tempDataMock;
    private readonly JoinOrganisationModel _joinOrganisationModel;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly string _identifier = "GB-COH:123456789";
    private readonly Guid _personId = Guid.NewGuid();
    private readonly CDP.Organisation.WebApiClient.Organisation _organisation;

    public JoinOrganisationModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _tempDataMock = new Mock<ITempDataService>();
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = _personId});
        _joinOrganisationModel = new JoinOrganisationModel(_organisationClientMock.Object, _sessionMock.Object, _tempDataMock.Object);
        _organisation = new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, _organisationId, null, "Test Org", []);
    }

    [Fact]
    public async Task OnGet_ValidOrganisationId_ReturnsPageResult()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationModel.OnGet(_identifier);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.LookupOrganisationAsync(string.Empty, _identifier), Times.Once);
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _joinOrganisationModel.OnGet(_identifier);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");

        _organisationClientMock.Verify(client => client.LookupOrganisationAsync(string.Empty, _identifier), Times.Once);
    }

    [Fact]
    public async Task OnPost_ModelStateInvalid_ReturnsPageResultWithOrganisationDetails()
    {
        _joinOrganisationModel.ModelState.AddModelError("Join", "Select an option");

        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationModel.OnPost(_identifier);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.LookupOrganisationAsync(string.Empty, _identifier), Times.Once);
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsTrue_CreatesJoinRequestAndRedirectsToSuccessPage()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        _joinOrganisationModel.Join = true;

        var result = await _joinOrganisationModel.OnPost(_identifier);

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(
            _organisationId,
            It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)),
            Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_identifier}/join-organisation/success");
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsFalse_RedirectsToCompaniesHouseNumber()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        _joinOrganisationModel.Join = false;

        var result = await _joinOrganisationModel.OnPost(_identifier);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/registration/has-companies-house-number");

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<CreateOrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_UserHasNoPersonId_RedirectsToHomePage()
    {
        _joinOrganisationModel.Join = true;
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = null});

        var result = await _joinOrganisationModel.OnPost(_identifier);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/");

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<CreateOrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_CreateJoinRequestThrowsApiException_SetsFlashMessageAndReturnsPage()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);
        _joinOrganisationModel.Join = true;

        _organisationClientMock.Setup(client => client.CreateJoinRequestAsync(
                _organisationId,
                It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)))
            .ThrowsAsync(new ApiException<OrganisationWebApiClient.ProblemDetails>("Error", 400, "Error", null!, null!, null!));

        var result = await _joinOrganisationModel.OnPost(_identifier);

        _tempDataMock.Verify(tempData => tempData.Put(
                FlashMessageTypes.Important,
                It.IsAny<FlashMessage>()),
            Times.Once);

        result.Should().BeOfType<PageResult>();
    }


}
