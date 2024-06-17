using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class AddressTypeQuestionModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly AddressTypeQuestionModel _model;

    public AddressTypeQuestionModelTests()
    {
        _sessionMock = SupplierDetailsFactory.CreateSessionMock();
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new AddressTypeQuestionModel(_sessionMock.Object, _organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidIdWithCompletedRegAddress_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(id);
        organisation.Addresses.Add(new Address(countryName: "United Kingdom", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", streetAddress2: "", type: AddressType.Registered));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(supplierInformation);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(organisation);

        var result = await _model.OnGet(id);

        result.Should().BeOfType<PageResult>();
        _model.HasUkPrincipalAddress.Should().Be(true);
    }


    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public void OnPost_ValidModelStateWithUkPrincipalAddress_ReturnsRedirectToPrincipalOfficeAddressUk()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasUkPrincipalAddress = true;

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("PrincipalOfficeAddressUk");
    }

    [Fact]
    public void OnPost_ValidModelStateWithoutUkPrincipalAddress_ReturnsRedirectToPrincipalOfficeAddressNonUk()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasUkPrincipalAddress = false;

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("PrincipalOfficeAddressNonUk");
    }

    [Fact]
    public void OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("HasUkPrincipalAddress", "Please select an option");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }
}