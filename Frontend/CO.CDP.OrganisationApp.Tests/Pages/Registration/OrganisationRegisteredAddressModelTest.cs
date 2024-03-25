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

public class OrganisationRegisteredAddressModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationRegisteredAddressModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(5);
    }

    [Fact]
    public void WhenAddressLine1IsEmpty_ShouldRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("AddressLine1")).First()
            .ErrorMessage.Should().Be("Enter your address line 1");
    }

    [Fact]
    public void WhenAddressLine1IsNotEmpty_ShouldNotRaiseAddressLine1ValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.AddressLine1 = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeFalse();
    }

    [Fact]
    public void WhenAddressLine2IsEmpty_ShouldRaiseAddressLine2ValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine2")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("AddressLine2")).First()
            .ErrorMessage.Should().Be("Enter your address line 2");
    }

    [Fact]
    public void WhenAddressLine2IsNotEmpty_ShouldNotRaiseAddressLine2ValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.AddressLine2 = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AddressLine2")).Should().BeFalse();
    }

    [Fact]
    public void WhenCityOrTownIsEmpty_ShouldRaiseCityOrTownValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("CityOrTown")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("CityOrTown")).First()
            .ErrorMessage.Should().Be("Enter your city or town");
    }

    [Fact]
    public void WhenCityOrTownIsNotEmpty_ShouldNotRaiseCityOrTownValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.CityOrTown = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("CityOrTown")).Should().BeFalse();
    }

    [Fact]
    public void WhenPostcodeIsEmpty_ShouldRaisePostcodeValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("Postcode")).First()
            .ErrorMessage.Should().Be("Enter your postcode or zip Code");
    }

    [Fact]
    public void WhenPostcodeIsNotEmpty_ShouldNotRaisePostcodeValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.Postcode = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeFalse();
    }



    [Fact]
    public void WhenCountryIsEmpty_ShouldRaiseCountryValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("Country")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("Country")).First()
            .ErrorMessage.Should().Be("Enter your country");
    }

    [Fact]
    public void WhenCountryIsNotEmpty_ShouldNotRaiseCountryValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.Country = "england";

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

        var model = GivenOrganisationDetailModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {        
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        model.AddressLine1.Should().Be(registrationDetails.OrganisationAddressLine1);
        model.AddressLine2.Should().Be(registrationDetails.OrganisationAddressLine2);
        model.CityOrTown.Should().Be(registrationDetails.OrganisationCityOrTown);
        model.Postcode.Should().Be(registrationDetails.OrganisationPostcode);
        model.Country.Should().Be(registrationDetails.OrganisationCountry);
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationAddressLine1 = "address line 1",
            OrganisationAddressLine2 = "address line 2",
            OrganisationCityOrTown = "london",
            OrganisationPostcode = "SW1A 2AA",
            OrganisationCountry = "england"
        };

        return registrationDetails;
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    private OrganisationRegisteredAddressModel GivenOrganisationDetailModel()
    {
        return new OrganisationRegisteredAddressModel(sessionMock.Object);
    }
}
