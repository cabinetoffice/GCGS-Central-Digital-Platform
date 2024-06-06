using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

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
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPost_DuplicateOrganisationName_AddsModelError()
    {
        var problemDetails = new OrganisationWebApiClient.ProblemDetails(
            title: "Duplicate organisation",
            detail: "An organisation with this name already exists.",
            status: 400,
            instance: null,
            type: null
        )
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                { "code", ErrorCodes.ORGANISATION_ALREADY_EXISTS }
            }
        };

        var aex = new ApiException<OrganisationWebApiClient.ProblemDetails>(
            "Duplicate organisation",
            400,
            "Bad Request",
            null,
            problemDetails,
            null
        );

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        Assert.NotNull(model.ModelState);
        var modelState = model.ModelState[string.Empty];
        Assert.NotNull(modelState);
        modelState.Errors.Should().Contain(e => e.ErrorMessage == ErrorMessagesList.DuplicateOgranisationName);
    }

    [Fact]
    public async Task OnPost_ArgumentNull_AddsModelError()
    {
        var problemDetails = new OrganisationWebApiClient.ProblemDetails(
            title: "Argument null",
            detail: "Argument cannot be null.",
            status: 400,
            instance: null,
            type: null
        )
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                { "code", ErrorCodes.ARGUMENT_NULL }
            }
        };

        var aex = new ApiException<OrganisationWebApiClient.ProblemDetails>(
            "Argument null",
            400,
            "Bad Request",
            null,
            problemDetails,
            null
        );

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        Assert.NotNull(model.ModelState);
        var modelState = model.ModelState[string.Empty];
        Assert.NotNull(modelState);
        modelState.Errors.Should().Contain(e => e.ErrorMessage == ErrorMessagesList.PayLoadIssueOrNullAurgument);
    }

    [Fact]
    public async Task OnPost_InvalidOperation_AddsModelError()
    {
        var problemDetails = new OrganisationWebApiClient.ProblemDetails(
            title: "Invalid operation",
            detail: "The operation is invalid.",
            status: 400,
            instance: null,
            type: null
        )
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                { "code", ErrorCodes.INVALID_OPERATION }
            }
        };

        var aex = new ApiException<OrganisationWebApiClient.ProblemDetails>(
            "Invalid operation",
            400,
            "Bad Request",
            null,
            problemDetails,
            null
        );

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        Assert.NotNull(model.ModelState);
        var modelState = model.ModelState[string.Empty];
        Assert.NotNull(modelState);
        modelState.Errors.Should().Contain(e => e.ErrorMessage == ErrorMessagesList.OrganisationCreationFailed);
    }

    [Fact]
    public async Task OnPost_PersonNotFound_AddsModelError()
    {
        var problemDetails = new OrganisationWebApiClient.ProblemDetails(
            title: "Person not found",
            detail: "The requested person was not found.",
            status: 404,
            instance: null,
            type: null
        )
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                { "code", ErrorCodes.PERSON_DOES_NOT_EXIST }
            }
        };

        var aex = new ApiException<OrganisationWebApiClient.ProblemDetails>(
            "Person not found",
            404,
            "Not Found",
            null,
            problemDetails,
            null
        );

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        Assert.NotNull(model.ModelState);
        var modelState = model.ModelState[string.Empty];
        Assert.NotNull(modelState);
        modelState.Errors.Should().Contain(e => e.ErrorMessage == ErrorMessagesList.PersonNotFound);
    }

    [Fact]
    public async Task OnPost_UnprocessableEntity_AddsModelError()
    {
        var problemDetails = new OrganisationWebApiClient.ProblemDetails(
            title: "Unprocessable entity",
            detail: "The provided data is not processable.",
            status: 422,
            instance: null,
            type: null
        )
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                { "code", ErrorCodes.UNPROCESSABLE_ENTITY }
            }
        };

        var aex = new ApiException<OrganisationWebApiClient.ProblemDetails>(
            "Unprocessable entity",
            422,
            "Unprocessable Entity",
            null,
            problemDetails,
            null
        );

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        Assert.NotNull(model.ModelState);
        var modelState = model.ModelState[string.Empty];
        Assert.NotNull(modelState);
        modelState.Errors.Should().Contain(e => e.ErrorMessage == ErrorMessagesList.UnprocessableEntity);
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