using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierAddressModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierAddressModel _model;

    public SupplierAddressModelTest()
    {
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new SupplierAddressModel(_organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
            AddressType = Constants.AddressType.Registered
        };
    }

    private void SetupModelWithUkAddress()
    {
        _model.Address = new OrganisationApp.Pages.Shared.AddressPartialModel
        {
            AddressLine1 = "1 London Street",
            TownOrCity = "London",
            Postcode = "L1",
            Country = "GB"
        };
    }

    private void SetupMockForValidGet()
    {
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id);
        organisation.Addresses.Add(new Address(countryName: "United Kingdom", country: "GB", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", type: AddressType.Registered));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(supplierInformation);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);
    }

    [Fact]
    public async Task OnGet_ValidIdWithUkAddress_ReturnsPageResult()
    {
        SetupMockForValidGet();

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Address.AddressLine1.Should().Be("1 London Street");
        _model.Address.TownOrCity.Should().Be("London");
        _model.Address.Postcode.Should().Be("L1");
        _model.Address.Country.Should().Be("GB");
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        SetupModelWithUkAddress();

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(OrganisationClientModel(_model.Id));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(SupplierInformationClientModel);

        _organisationClientMock.Setup(client => client.UpdateOrganisationAsync(_model.Id, It.IsAny<UpdatedOrganisation>()))
            .Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        var id = Guid.NewGuid();
        _model.Id = id;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierInformationClientModel);

        _model.ModelState.AddModelError("AddressLine1", "Enter your address line 1");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        SetupModelWithUkAddress();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static SupplierInformation SupplierInformationClientModel => new(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: false,
            completedWebsiteAddress: false,
            completedEmailAddress: true,
            completedOperationType: false,
            completedLegalForm: false,
            completedConnectedPerson: false,
            legalForm: null);

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            contactPoint: new ContactPoint(email: "test@test.com", name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );
}