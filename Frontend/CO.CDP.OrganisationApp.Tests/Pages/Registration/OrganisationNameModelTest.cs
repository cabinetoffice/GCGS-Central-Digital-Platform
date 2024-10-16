using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ThirdPartyApiClients;
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
            .ErrorMessage.Should().Be("OrganisationRegistration_EnterOrganisationName_Heading");    // Not passed through localization at this point
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
    public async Task OnGet_WhenCompaniesHouseNumberProvided_ShouldPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");
        var model = GivenOrganisationNameModel();

        companiesHouseMock.Setup(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(profile);

        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberAndCompanyNameProvided_ShouldNotPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails("Microsoft Limited");
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();
        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");

        companiesHouseMock.Setup(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(profile);
        
        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
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

    private RegistrationDetails DummyRegistrationDetails(string organisationName = "TestOrg")
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationIdentificationNumber = "123456",
            OrganisationHasCompaniesHouseNumber = true,
        };

        return registrationDetails;
    }

    private CompanyProfile GivenProfileOnCompaniesHouse(string organisationName = "")
    {
        return new CompanyProfile() {  CompanyName = organisationName };
    }

    private RegisteredAddress GivenRegisteredAddressOnCompaniesHouse()
    {
        return new RegisteredAddress()
        {
            AddressLine1 = "100 Park Lane",
            AddressLine2 = "",
            Country = "United Kingdom",
            Locality = "London",
            PostalCode = "SW1 1LP"
        };
    }

    private OrganisationNameModel GivenOrganisationNameModel()
    {
        return new OrganisationNameModel(sessionMock.Object, companiesHouseMock.Object);
    }
}