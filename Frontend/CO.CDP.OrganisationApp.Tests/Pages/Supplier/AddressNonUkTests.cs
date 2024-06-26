using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class AddressNonUkModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly AddressNonUkModel _model;

    public AddressNonUkModelTests()
    {
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new AddressNonUkModel(_organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
            AddressType = Constants.AddressType.Registered
        };
    }

    [Fact]
    public async Task OnGet_ValidIdWithNonUkAddress_ReturnsPageResult()
    {
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id);
        organisation.Addresses.Add(new Address(countryName: "France", locality: "Paris", postalCode: "F1", region: "North", streetAddress: "1 Paris Street", streetAddress2: "", type: AddressType.Registered));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(supplierInformation);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.AddressLine1.Should().Be("1 Paris Street");
        _model.TownOrCity.Should().Be("Paris");
        _model.Region.Should().Be("North");
        _model.Postcode.Should().Be("F1");
        _model.Country.Should().Be("France");
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        _model.AddressLine1 = "1 Paris Street";
        _model.TownOrCity = "Paris";
        _model.Postcode = "F1";
        _model.Country = "France";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id));

        _organisationClientMock.Setup(client => client.UpdateOrganisationAsync(_model.Id, It.IsAny<UpdatedOrganisation>()))
            .Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("AddressLine1", "Enter your address line 1");
        var result = await _model.OnPost();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _model.AddressLine1 = "1 Paris Street";
        _model.TownOrCity = "Paris";
        _model.Postcode = "F1";
        _model.Country = "France";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}