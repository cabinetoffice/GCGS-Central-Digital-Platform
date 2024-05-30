using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationNonUKAddressModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationNonUKAddressModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(4);
    }

    [Fact]
    public void WhenAddressLine1IsEmpty_ShouldRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("AddressLine1")).First()
            .ErrorMessage.Should().Be("Enter your address line 1");
    }

    [Fact]
    public void WhenAddressLine1IsNotEmpty_ShouldNotRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.AddressLine1 = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeFalse();
    }

    [Fact]
    public void WhenAddressLine2IsNotEmpty_ShouldNotRaiseAddressLine2ValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.AddressLine2 = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine2")).Should().BeFalse();
    }

    [Fact]
    public void WhenTownOrCityIsEmpty_ShouldRaiseTownOrCityValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("TownOrCity")).First()
            .ErrorMessage.Should().Be("Enter your town or city");
    }

    [Fact]
    public void WhenTownOrCityIsNotEmpty_ShouldNotRaiseTownOrCityValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.TownOrCity = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeFalse();
    }

    [Fact]
    public void WhenPostcodeIsEmpty_ShouldRaisePostcodeValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("Postcode")).First()
            .ErrorMessage.Should().Be("Enter your postcode");
    }

    [Fact]
    public void WhenPostcodeIsNotEmpty_ShouldNotRaisePostcodeValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Postcode = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeFalse();
    }

    [Fact]
    public void WhenCountryIsEmpty_ShouldRaiseCountryValidationError()
    {
        var model = GivenOrganisationAddressModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Country")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("Country")).First()
            .ErrorMessage.Should().Be("Enter your country");
    }

    [Fact]
    public void WhenCountryIsNotEmpty_ShouldNotRaiseCountryValidationError()
    {
        var model = GivenOrganisationAddressModel();
        model.Country = "India";

        var results = ModelValidationHelper.Validate(model);

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
    public void OnPost_WhenValidModelAndSupplier_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenOrganisationAddressModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationType = OrganisationType.Supplier;
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnPost_WhenValidModelAndBuyer_ShouldRedirectToBuyerOrganisationTypePage()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        registrationDetails.OrganisationType = OrganisationType.Buyer;
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("BuyerOrganisationType");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationAddressModel();

        model.OnGet();

        model.AddressLine1.Should().Be(registrationDetails.OrganisationAddressLine1);
        model.AddressLine2.Should().Be(registrationDetails.OrganisationAddressLine2);
        model.TownOrCity.Should().Be(registrationDetails.OrganisationCityOrTown);
        model.Postcode.Should().Be(registrationDetails.OrganisationPostcode);
        model.Country.Should().Be(registrationDetails.OrganisationCountry);
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationAddressLine1 = "address line 1",
            OrganisationAddressLine2 = "address line 2",
            OrganisationCityOrTown = "New Delhi",
            OrganisationRegion = "NCR",
            OrganisationPostcode = "0123456789",
            OrganisationCountry = "India"
        };

        return registrationDetails;
    }

    private OrganisationNonUKAddressModel GivenOrganisationAddressModel()
    {
        return new OrganisationNonUKAddressModel(sessionMock.Object);
    }
}