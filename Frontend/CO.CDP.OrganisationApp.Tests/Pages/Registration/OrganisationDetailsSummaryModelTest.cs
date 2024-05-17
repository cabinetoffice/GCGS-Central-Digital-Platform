using CO.CDP.Common.Enums;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

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

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();
        model.Details.As<RegistrationDetails>().Should().NotBeNull();
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Action action = model.OnGet;
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public async Task OnPost_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Func<Task> act = model.OnPost;
        await act.Should().ThrowAsync<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public async Task OnPost_ValidSession_ShouldRegisterOrganisation()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(DummyRegistrationDetails());

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenErrorInRegisteringOrganisation_ShouldReturnPageWithError()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(DummyRegistrationDetails());

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(new ApiException("Unexpected error", 500, "", default, null));

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToOrganisationOverview()
    {
        var registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            UserUrn = "urn:fdc:gov.uk:2022:6ba2a483e56144418756fd2edf1a55f6",
            PersonId = Guid.NewGuid(),
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
        return new OrganisationDetailsSummaryModel(sessionMock.Object, organisationClientMock.Object)
        { Details = DummyRegistrationDetails()};
    }
}
