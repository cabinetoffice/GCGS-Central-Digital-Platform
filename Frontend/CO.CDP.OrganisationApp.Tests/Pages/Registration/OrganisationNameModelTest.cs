using CO.CDP.Localization;
using CO.CDP.OrganisationApp.CharityCommission;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ThirdPartyApiClients;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationNameModelTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<ICompaniesHouseApi> _companiesHouseMock;
    private readonly Mock<ICharityCommissionApi> _charityCommissionMock;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;

    public OrganisationNameModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _companiesHouseMock = new Mock<ICompaniesHouseApi>();
        _charityCommissionMock = new Mock<ICharityCommissionApi>();
        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
        _stringLocalizerMock = new Mock<IStringLocalizer>();
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

        _stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading)])
            .Returns(new LocalizedString(nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading), StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading));

        var validationContext = ValidationContextFactory.GivenValidationContextWithStringLocalizerFactory(model, _stringLocalizerMock.Object);

        var results = ModelValidationHelper.Validate(model, validationContext);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("OrganisationName")).First()
            .ErrorMessage.Should().Be(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading);    // Not passed through localization at this point
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
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).
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

        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        _sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        _sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();
        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberProvided_ShouldPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails();
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");
        var model = GivenOrganisationNameModel();

        _companiesHouseMock.Setup(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(profile);

        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberAndCompanyNameProvided_ShouldNotPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails("Microsoft Limited");
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameModel();
        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");

        _companiesHouseMock.Setup(ch => ch.GetProfile(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(profile);
        
        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberProvided_ShouldPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails(organisationName: "", scheme: OrganisationSchemeType.CharityCommissionEnglandWales);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var chartiyDetails = GivenNameOnCharitiesCommission(organisationName: "British Red Cross");
        var model = GivenOrganisationNameModel();

        _charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.OrganisationName.Should().Be(chartiyDetails.Name);
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberProvidedAndRegDetailsProvided_ShouldNotPrepopulateCompanyName()
    {
        var registrationDetails = DummyRegistrationDetails(organisationName: "British Heart Foundation", scheme: OrganisationSchemeType.CharityCommissionEnglandWales);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var chartiyDetails = GivenNameOnCharitiesCommission(organisationName: "British Red Cross");
        var model = GivenOrganisationNameModel();

        _charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.OrganisationName.Should().Be(registrationDetails.OrganisationName);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationNameSearchPage()
    {
        var model = GivenOrganisationNameModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationNameSearch");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenOrganisationNameModel();
        model.RedirectToSummary = true;

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private RegistrationDetails DummyRegistrationDetails(string organisationName = "TestOrg", string scheme = "")
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = organisationName,
            OrganisationScheme = scheme,
            OrganisationEmailAddress = "test@example.com",
            OrganisationIdentificationNumber = "123456",
            OrganisationHasCompaniesHouseNumber = true
        };

        return registrationDetails;
    }

    private CompanyProfile GivenProfileOnCompaniesHouse(string organisationName = "")
    {
        return new CompanyProfile() {  CompanyName = organisationName };
    }

    private CharityDetails GivenNameOnCharitiesCommission(string organisationName = "")
    {
        return new CharityDetails()
        {
            Name = organisationName
        };
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
        return new OrganisationNameModel(_sessionMock.Object, _charityCommissionMock.Object, _companiesHouseMock.Object);
    }
}