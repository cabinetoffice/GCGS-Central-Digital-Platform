using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class PrincipalOfficeAddressNonUkModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly PrincipalOfficeAddressNonUkModel _model;

    public PrincipalOfficeAddressNonUkModelTests()
    {
        _sessionMock = SupplierDetailsFactory.CreateSessionMock();
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new PrincipalOfficeAddressNonUkModel(_sessionMock.Object, _organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidIdWithNonUkAddress_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(id);
        organisation.Addresses.Add(new Address(countryName: "France", locality: "Paris", postalCode: "F1", region: "North", streetAddress: "1 Paris Street", streetAddress2: "", type: AddressType.Registered));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(supplierInformation);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(organisation);

        var result = await _model.OnGet(id);

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
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.AddressLine1 = "1 Paris Street";
        _model.TownOrCity = "Paris";
        _model.Postcode = "F1";
        _model.Country = "France";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.UpdateOrganisationAsync(id, It.IsAny<UpdatedOrganisation>()))
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
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.AddressLine1 = "1 Paris Street";
        _model.TownOrCity = "Paris";
        _model.Postcode = "F1";
        _model.Country = "France";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}
