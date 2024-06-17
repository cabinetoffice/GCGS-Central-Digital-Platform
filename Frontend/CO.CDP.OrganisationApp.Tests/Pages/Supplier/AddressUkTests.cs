using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class AddressUkModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly AddressUkModel _model;

    public AddressUkModelTests()
    {
        _sessionMock = SupplierDetailsFactory.CreateSessionMock();
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new AddressUkModel(_sessionMock.Object, _organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
            AddressType = Constants.AddressType.Registered
        };
    }

    private void SetupModelWithUkAddress()
    {
        _model.AddressLine1 = "1 London Street";
        _model.AddressLine2 = "";
        _model.TownOrCity = "London";
        _model.Postcode = "L1";
        _model.Country = "United Kingdom";
    }

    private void SetupMockForValidGet()
    {
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id);
        organisation.Addresses.Add(new Address(countryName: "United Kingdom", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", streetAddress2: "", type: AddressType.Registered));

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
        _model.AddressLine1.Should().Be("1 London Street");
        _model.TownOrCity.Should().Be("London");
        _model.Postcode.Should().Be("L1");
        _model.Country.Should().Be("United Kingdom");
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
        SetupModelWithUkAddress();

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }
}