using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.Tests.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationRegisteredAddressModelTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly OrganisationRegisteredAddressModel _model;

    public OrganisationRegisteredAddressModelTest()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        _model = new OrganisationRegisteredAddressModel(organisationClientMock.Object);
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
            .ErrorMessage.Should().Be("Enter your address line 1");
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
            .ErrorMessage.Should().Be("Enter your town or city");
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


        _model.PageContext = pageContext;

        var actionResult = _model.OnPost();

        actionResult.Should().BeOfType<Task<IActionResult>>();
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldSaveRegistratedAddress()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.Address = new AddressPartialModel()
        {
            AddressLine1 = "2 street lane",
            TownOrCity = "Leeds",
            Country = "GB",
            Postcode = "LS1 2AE"

        };
        organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
           .Which.PageName.Should().Be("OrganisationOverview");        
    }    

    [Fact]
    public async Task OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await _model.OnGet();

        organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }


    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        List<CO.CDP.Organisation.WebApiClient.Address> addresses = new();

        addresses.Add(new CO.CDP.Organisation.WebApiClient.Address(
            countryName: "United Kingdom",
            country: "GB",
            locality: "Leeds",
            postalCode: "LS1 2AE",
            region: null,
            streetAddress: "1 street lane",
            type: CDP.Organisation.WebApiClient.AddressType.Registered));

        return new CO.CDP.Organisation.WebApiClient.Organisation(null, addresses, null, id!.Value, null, null, []);
    }

    private OrganisationRegisteredAddressModel GivenOrganisationAddressModel()
    {
        return new OrganisationRegisteredAddressModel(organisationClientMock.Object) { Address = new() };
    }
}