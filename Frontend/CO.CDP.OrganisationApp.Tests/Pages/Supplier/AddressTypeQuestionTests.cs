using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class AddressTypeQuestionModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly AddressTypeQuestionModel _model;

    public AddressTypeQuestionModelTests()
    {
        _organisationClientMock = SupplierDetailsFactory.CreateOrganisationClientMock();
        _model = new AddressTypeQuestionModel(_organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
            AddressType = Constants.AddressType.Registered
        };
    }

    [Fact]
    public async Task OnGet_ValidIdWithCompletedRegAddress_ReturnsPageResult()
    {
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(_model.Id);
        organisation.Addresses.Add(new Address(countryName: "United Kingdom", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", streetAddress2: "", type: AddressType.Registered));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(supplierInformation);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.HasUkAddress.Should().Be(true);
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
    public void OnPost_ValidModelStateWithUkAddress_ReturnsRedirectToAddressUk()
    {
        _model.HasUkAddress = true;

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("AddressUk");
    }

    [Fact]
    public void OnPost_ValidModelStateWithoutUkAddress_ReturnsRedirectToAddressNonUk()
    {
        _model.HasUkAddress = false;

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("AddressNonUk");
    }

    [Fact]
    public void OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("HasUkAddress", "Please select an option");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }
}