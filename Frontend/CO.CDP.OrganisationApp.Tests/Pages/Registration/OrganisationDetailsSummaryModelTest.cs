using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using static CO.CDP.OrganisationApp.Tests.Pages.Registration.OrganisationEntityFactory;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationDetailsSummaryModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public OrganisationDetailsSummaryModelTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenOrganisationDetailModel();

        model.OnGet();

        model.RegistrationDetails.As<RegistrationDetails>().Should().NotBeNull();
    }

    [Fact]
    public async Task OnPost_ValidSession_ShouldRegisterOrganisation()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToOrganisationOverview()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/OrganisationOverview");
    }

    [Fact]
    public async Task OnPost_DuplicateOrganisationName_AddsModelError()
    {
        var problemDetails = GivenProblemDetails(statusCode: 400, code: ErrorCodes.ORGANISATION_ALREADY_EXISTS);
        var aex = GivenApiException(statusCode: 400, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();
        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
          .Should().Contain(e => e.ErrorMessage == ErrorMessagesList.DuplicateOgranisationName);
    }

    [Fact]
    public async Task OnPost_ArgumentNull_AddsModelError()
    {
        var problemDetails = GivenProblemDetails(code: ErrorCodes.ARGUMENT_NULL, statusCode: 400);
        var aex = GivenApiException(statusCode: 400, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
            .Should().Contain(e => e.ErrorMessage == ErrorMessagesList.PayLoadIssueOrNullAurgument);
    }

    [Fact]
    public async Task OnPost_InvalidOperation_AddsModelError()
    {
        var problemDetails = GivenProblemDetails(code: ErrorCodes.INVALID_OPERATION, statusCode: 400);
        var aex = GivenApiException(statusCode: 400, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
           .Should().Contain(e => e.ErrorMessage == ErrorMessagesList.OrganisationCreationFailed);
    }

    [Fact]
    public async Task OnPost_PersonNotFound_AddsModelError()
    {
        var problemDetails = GivenProblemDetails(code: ErrorCodes.PERSON_DOES_NOT_EXIST, statusCode: 404);
        var aex = GivenApiException(statusCode: 404, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
          .Should().Contain(e => e.ErrorMessage == ErrorMessagesList.PersonNotFound);
    }

    [Fact]
    public async Task OnPost_UnprocessableEntity_AddsModelError()
    {
        var problemDetails = GivenProblemDetails(code: ErrorCodes.UNPROCESSABLE_ENTITY, statusCode: 422);
        var aex = GivenApiException(statusCode: 422, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
         .Should().Contain(e => e.ErrorMessage == ErrorMessagesList.UnprocessableEntity);
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationType = OrganisationType.Supplier
        };

        return registrationDetails;
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, Guid.NewGuid(), null, "Test Org", []);
    }

    private OrganisationDetailsSummaryModel GivenOrganisationDetailModel()
    {
        var registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        return new OrganisationDetailsSummaryModel(sessionMock.Object, organisationClientMock.Object);
    }

}