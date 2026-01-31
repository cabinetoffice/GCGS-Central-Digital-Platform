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
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationRegisteredAddressModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<ICompaniesHouseApi> companiesHouseMock;
    private readonly Mock<ICharityCommissionApi> charityCommissionMock;

    public OrganisationRegisteredAddressModelTest()
    {
        sessionMock = new Mock<ISession>();
        companiesHouseMock = new Mock<ICompaniesHouseApi>();
        charityCommissionMock = new Mock<ICharityCommissionApi>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Count.Should().Be(4);
    }

    [Fact]
    public void WhenAddressLine1IsEmpty_ShouldRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("AddressLine1")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.Shared_Address_AddressLine1_ErrorMessage));
    }

    [Fact]
    public void WhenAddressLine1IsNotEmpty_ShouldNotRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Address.AddressLine1 = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeFalse();
    }

    [Fact]
    public void WhenTownOrCityIsEmpty_ShouldRaiseTownOrCityValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("TownOrCity")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.Shared_Address_TownOrCity_ErrorMessage));
    }

    [Fact]
    public void WhenTownOrCityIsNotEmpty_ShouldNotRaiseTownOrCityValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Address.TownOrCity = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeFalse();
    }

    [Fact]
    public void WhenPostcodeIsNotEmpty_ShouldNotRaisePostcodeValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Address.Postcode = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeFalse();
    }

    [Fact]
    public void WhenCountryIsNotEmpty_ShouldNotRaiseCountryValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Address.Country = "EN";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("Country")).Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenOrganisationAddressModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationAddressModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModelAndSupplier_ShouldRedirectToSupplierOrganisationTypePage()
    {
        var model = GivenOrganisationAddressModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationType = OrganisationType.Supplier;
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierOrganisationType");
    }

    [Fact]
    public void OnPost_WhenValidModelAndBuyer_ShouldRedirectToBuyerOrganisationTypePage()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationType = OrganisationType.Buyer;
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("BuyerOrganisationType");
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();

        await model.OnGet();

        model.Address.AddressLine1.Should().Be(registrationDetails.OrganisationAddressLine1);
        model.Address.TownOrCity.Should().Be(registrationDetails.OrganisationCityOrTown);
        model.Address.Postcode.Should().Be(registrationDetails.OrganisationPostcode);
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberProvided_ShouldPrepopulateRegisteredAddress()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails(emptyAddress: true,
            scheme: OrganisationSchemeType.CharityCommissionEnglandWales, hasCompaniesHouseNumber: false);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();
        var chartiyDetails = GivenRegisteredAddressOnCharitiesCommission();

        charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.Address.AddressLine1.Should().Be(chartiyDetails.AddressLine2);
        model.Address.TownOrCity.Should().Be(chartiyDetails.AddressLine3);
        model.Address.Postcode.Should().Be(chartiyDetails.PostalCode);
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberAndRegDetailsProvided_ShouldNotPrepopulateRegisteredAddress()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails(scheme: OrganisationSchemeType.CharityCommissionEnglandWales,
            hasCompaniesHouseNumber: false);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();
        var chartiyDetails = GivenRegisteredAddressOnCharitiesCommission();

        charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.Address.AddressLine1.Should().Be(registrationDetails.OrganisationAddressLine1);
        model.Address.TownOrCity.Should().Be(registrationDetails.OrganisationCityOrTown);
        model.Address.Postcode.Should().Be(registrationDetails.OrganisationPostcode);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberProvided_ShouldPrepopulateRegisteredAddress()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails(emptyAddress: true);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();
        var companiesHouseRegisteredAddress = GivenRegisteredAddressOnCompaniesHouse();

        companiesHouseMock.Setup(ch => ch.GetRegisteredAddress(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(companiesHouseRegisteredAddress);

        await model.OnGet();

        model.Address.AddressLine1.Should().Be(companiesHouseRegisteredAddress.AddressLine1);
        model.Address.TownOrCity.Should().Be(companiesHouseRegisteredAddress.Locality);
        model.Address.Postcode.Should().Be(companiesHouseRegisteredAddress.PostalCode);
    }

    [Fact]
    public async Task OnGet_WhenCompaniesHouseNumberAndRegDetailsProvided_ShouldNotPrepopulateRegisteredAddress()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();
        var companiesHouseRegisteredAddress = GivenRegisteredAddressOnCompaniesHouse();

        companiesHouseMock.Setup(ch => ch.GetRegisteredAddress(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(companiesHouseRegisteredAddress);

        await model.OnGet();

        model.Address.AddressLine1.Should().Be(registrationDetails.OrganisationAddressLine1);
        model.Address.TownOrCity.Should().Be(registrationDetails.OrganisationCityOrTown);
        model.Address.Postcode.Should().Be(registrationDetails.OrganisationPostcode);
    }

    private RegistrationDetails DummyRegistrationDetails(bool emptyAddress = false, string scheme = "", bool hasCompaniesHouseNumber = true)
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationAddressLine1 = emptyAddress ? "": "address line 1",
            OrganisationCityOrTown = emptyAddress ? "" : "london",
            OrganisationPostcode = emptyAddress ? "" : "SW1A 2AA",
            OrganisationIdentificationNumber = "123456",
            OrganisationHasCompaniesHouseNumber = true,
            OrganisationCountryName = "United Kingdom",
            OrganisationCountryCode = "GB",
            OrganisationScheme = scheme
        };

        return registrationDetails;
    }

    private RegisteredAddress GivenRegisteredAddressOnCompaniesHouse()
    {
        return new RegisteredAddress()
        {
            AddressLine1 = "10 Park Lane",
            AddressLine2 = "",
            Country = "UK",
            Locality = "London",
            PostalCode = "SW1 1AP"
        };
    }

    private CharityDetails GivenRegisteredAddressOnCharitiesCommission()
    {
        return new CharityDetails()
        {
            AddressLine1 = "1 Mayfair",
            AddressLine2 = "",
            PostalCode = "SW2 1PT"
        };
    }

    private OrganisationRegisteredAddressModel GivenOrganisationAddressModel()
    {
        return new OrganisationRegisteredAddressModel(sessionMock.Object, charityCommissionMock.Object, companiesHouseMock.Object) { Address = new() };
    }
}