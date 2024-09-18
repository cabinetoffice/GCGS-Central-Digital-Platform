using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationNameModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<ICompaniesHouseApi> companiesHouseMock;

    public OrganisationNameModelTest()
    {
        sessionMock = new Mock<ISession>();
        companiesHouseMock = new Mock<ICompaniesHouseApi>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationNameModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenOrganisationNameIsEmpty_ShouldRaiseOrganisationNameValidationError()
    {
        var model = GivenOrganisationNameModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("OrganisationName")).First()
            .ErrorMessage.Should().Be("Enter the organisation's name");
    }

    [Fact]
    public void WhenOrganisationNameIsNotEmpty_ShouldNotRaiseOrganisationNameValidationError()
    {
        var model = GivenOrganisationNameModel();
        model.OrganisationName = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).
            Returns(DummyRegistrationDetails());
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenOrganisationNameModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationNameModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();
        model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberProvided_ShouldCallCompaniesHouseApi()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationHasCompaniesHouseNumber = true;
        registrationDetails.OrganisationIdentificationNumber = "0123456789";
        registrationDetails.OrganisationName = string.Empty;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();

        await model.OnGet();

        companiesHouseMock.Verify(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberAndCompanyNameProvided_ShouldNotCallCompaniesHouseApi()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationHasCompaniesHouseNumber = true;
        registrationDetails.OrganisationIdentificationNumber = "0123456789";
        registrationDetails.OrganisationName = "Acme Ltd";

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();

        await model.OnGet();

        companiesHouseMock.Verify(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber), Times.Never);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationEmailPage()
    {
        var model = GivenOrganisationNameModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationEmail");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenOrganisationNameModel();
        model.RedirectToSummary = true;

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com"
        };

        return registrationDetails;
    }

    private OrganisationNameModel GivenOrganisationNameModel()
    {
        return new OrganisationNameModel(sessionMock.Object, companiesHouseMock.Object);
    }
}