using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using Moq;
using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class JoinOrganisationModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IFlashMessageService> flashMessageServiceMock;
    private readonly JoinOrganisationModel _joinOrganisationModel;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly string _identifier = "GB-COH:123456789";
    private readonly Guid _personId = Guid.NewGuid();
    private readonly CDP.Organisation.WebApiClient.Organisation _organisation;

    public JoinOrganisationModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        flashMessageServiceMock = new Mock<IFlashMessageService>();
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = _personId });

        _joinOrganisationModel = new JoinOrganisationModel(_organisationClientMock.Object, _sessionMock.Object, flashMessageServiceMock.Object);
        _organisation = new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, _organisationId, null, "Test Org", [], OrganisationWebApiClient.OrganisationType.Organisation);
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
        _joinOrganisationModel.ModelState.AddModelError("UserWantsToJoin", StaticTextResource.OrganisationRegistration_JoinOrganisation_ValidationErrorMessage);

        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        var result = await _joinOrganisationModel.OnPost(_identifier);

        result.Should().BeOfType<PageResult>();
        _joinOrganisationModel.OrganisationDetails.Should().Be(_organisation);
        _organisationClientMock.Verify(client => client.LookupOrganisationAsync(string.Empty, _identifier), Times.Once);
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsTrue_UserConfirmationConfirmed_CreatesJoinRequestRemovedRegistrationSessionAndRedirectsToSuccessPage()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        _joinOrganisationModel.UserWantsToJoin = true;
        _joinOrganisationModel.UserConfirmation = "Confirmed";

        _organisationClientMock.Setup(client => client.CreateJoinRequestAsync(_organisationId,
            It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)))
            .ReturnsAsync(new OrganisationJoinRequest(Guid.NewGuid(), _organisation, null, requestCreated: true, null, null, OrganisationJoinRequestStatus.Pending));

        ValidateModel(_joinOrganisationModel);

        var result = await _joinOrganisationModel.OnPost(_identifier);

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(
            _organisationId,
            It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)),
            Times.Once);

        _sessionMock.Verify(s => s.Remove(Session.RegistrationDetailsKey), Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_identifier}/join-organisation/success");
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsPending_ShowsPendingMmberFlashMessage()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        _joinOrganisationModel.UserWantsToJoin = true;
        _joinOrganisationModel.UserConfirmation = "Confirmed";

        _organisationClientMock.Setup(client => client.CreateJoinRequestAsync(_organisationId,
            It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)))
            .ReturnsAsync(new OrganisationJoinRequest(Guid.NewGuid(), _organisation, null, requestCreated: false, null, null, OrganisationJoinRequestStatus.Pending));

        ValidateModel(_joinOrganisationModel);

        var result = await _joinOrganisationModel.OnPost(_identifier);

        flashMessageServiceMock.Verify(fms => fms.SetFlashMessage(
                FlashMessageType.Failure,
                StaticTextResource.OrganisationRegistration_JoinOrganisation_PendingMemberOfOrganisation,
                null,
                null,
                null,
                null),
            Times.Once);

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_UserHasPersonId_JoinIsFalse_RedirectsToMyAccount()
    {
        _organisationClientMock.Setup(client => client.LookupOrganisationAsync(string.Empty, _identifier))
            .ReturnsAsync(_organisation);

        _joinOrganisationModel.UserWantsToJoin = false;

        var result = await _joinOrganisationModel.OnPost(_identifier);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/organisation-selection");

        _organisationClientMock.Verify(client => client.CreateJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<CreateOrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_UserHasNoPersonId_RedirectsToHomePage()
    {
        _joinOrganisationModel.UserWantsToJoin = true;
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { UserUrn = "testUserUrn", PersonId = null });

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
        _joinOrganisationModel.UserWantsToJoin = true;

        _organisationClientMock.Setup(client => client.CreateJoinRequestAsync(
                _organisationId,
                It.Is<CreateOrganisationJoinRequest>(r => r.PersonId == _joinOrganisationModel.UserDetails.PersonId)))
            .ThrowsAsync(new ApiException<OrganisationWebApiClient.ProblemDetails>("Error", 400, "Error", null!, null!, null!));

        var result = await _joinOrganisationModel.OnPost(_identifier);

        flashMessageServiceMock.Verify(api => api.SetFlashMessage(
            FlashMessageType.Important,
            StaticTextResource.OrganisationRegistration_JoinOrganisation_AlreadyMemberOfOrganisation,
            null,
            null,
            null,
            null
        ),
        Times.Once);

        result.Should().BeOfType<PageResult>();
    }

    private void ValidateModel(JoinOrganisationModel model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);

        if (validationResults.Any())
            model.ModelState.AddModelError("Error", validationResults[0].ErrorMessage ?? "");
    }
}
