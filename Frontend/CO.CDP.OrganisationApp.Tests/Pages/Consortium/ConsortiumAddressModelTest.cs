using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumAddressModelTest
{
    private readonly Mock<ISession> _sessionMock;

    public ConsortiumAddressModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenConsortiumAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Count.Should().Be(4);
    }

    [Fact]
    public void WhenAddressLine1IsEmpty_ShouldRaiseAddressLine1ValidationError()
    {
        var model = GivenConsortiumAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("AddressLine1")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.Shared_Address_AddressLine1_ErrorMessage));
    }

    [Fact]
    public void WhenAddressLine1IsNotEmpty_ShouldNotRaiseAddressLine1ValidationError()
    {
        var model = GivenConsortiumAddressModel();
        model.Address.AddressLine1 = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("AddressLine1")).Should().BeFalse();
    }

    [Fact]
    public void WhenTownOrCityIsEmpty_ShouldRaiseTownOrCityValidationError()
    {
        var model = GivenConsortiumAddressModel();

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("TownOrCity")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.Shared_Address_TownOrCity_ErrorMessage));
    }

    [Fact]
    public void WhenTownOrCityIsNotEmpty_ShouldNotRaiseTownOrCityValidationError()
    {
        var model = GivenConsortiumAddressModel();
        model.Address.TownOrCity = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("TownOrCity")).Should().BeFalse();
    }

    [Fact]
    public void WhenPostcodeIsNotEmpty_ShouldNotRaisePostcodeValidationError()
    {
        var model = GivenConsortiumAddressModel();
        model.Address.Postcode = "dummay";

        var results = ModelValidationHelper.Validate(model.Address);

        results.Any(c => c.MemberNames.Contains("Postcode")).Should().BeFalse();
    }

    [Fact]
    public void WhenCountryIsNotEmpty_ShouldNotRaiseCountryValidationError()
    {
        var model = GivenConsortiumAddressModel();
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

        var model = GivenConsortiumAddressModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetConsortiumDetailsInSession()
    {
        var model = GivenConsortiumAddressModel();

        var consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey)).Returns(consortiumDetails);

        model.OnPost();

        _sessionMock.Verify(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey), Times.Once);
        _sessionMock.Verify(s => s.Set(Session.ConsortiumKey, It.IsAny<ConsortiumDetails>()), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModelAndBuyer_ShouldRedirectToConsortiumEmailPage()
    {
        var consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(consortiumDetails);

        var model = GivenConsortiumAddressModel();

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumEmail");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsConsortiumDetails()
    {
        var consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(consortiumDetails);
        _sessionMock.Setup(s => s.Set(Session.ConsortiumKey, consortiumDetails));

        var model = GivenConsortiumAddressModel();

        model.OnGet();

        model.Address.AddressLine1.Should().Be(consortiumDetails.PostalAddress!.AddressLine1);
        model.Address.TownOrCity.Should().Be(consortiumDetails.PostalAddress.TownOrCity);
        model.Address.Postcode.Should().Be(consortiumDetails.PostalAddress.Postcode);
    }
    
    private ConsortiumDetails DummyConsortiumDetails(string consortiumName = "Consortium 1", string consortiumEmailAddress = "test@example.com")
    {
        var consortiumDetails = new ConsortiumDetails
        {
            ConstortiumName = consortiumName,
            ConstortiumEmail = consortiumEmailAddress,
            PostalAddress = new Models.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", CountryName = "United kindom", Country = "GB" }
        };

        return consortiumDetails;
    }

    private ConsortiumAddressModel GivenConsortiumAddressModel()
    {
        return new ConsortiumAddressModel(_sessionMock.Object) { Address = new() };
    }
}