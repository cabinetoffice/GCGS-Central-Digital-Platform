using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class AddressPostalSameAsRegisteredQuestionTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly AddressPostalSameAsRegisteredQuestionModel _model;
    private readonly Guid _organisationId;

    public AddressPostalSameAsRegisteredQuestionTest()
    {
        _organisationId = Guid.NewGuid();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new AddressPostalSameAsRegisteredQuestionModel(_organisationClientMock.Object)
        {
            Id = _organisationId
        };
    }

    [Fact]
    public async Task OnGet_SelectedIsFalse_SameAsRegiseterdAddressShouldBeFalse()
    {
        var result = await _model.OnGet(false);

        _model.SameAsRegiseterdAddress.Should().BeFalse();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(null);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_OrganisationFound_SameAsRegiseterdAddressShouldBeSet()
    {
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(_organisationId));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(_organisationId))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel(completedPostalAddress: true));

        var result = await _model.OnGet(null);

        _model.SameAsRegiseterdAddress.Should().BeFalse();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ModelStateIsInvalid_ShouldReturnPageResult()
    {
        _model.ModelState.AddModelError("SameAsRegiseterdAddress", "Please select an option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_SameAsRegiseterdAddressIsTrue_ShouldUpdateOrganisationAddresses()
    {
        _model.SameAsRegiseterdAddress = true;

        var organisation = SupplierDetailsFactory.GivenOrganisationClientModel(_organisationId);
        _organisationClientMock.Setup(client => client.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        var result = await _model.OnPost();

        _organisationClientMock.Verify(o => o.UpdateOrganisationAsync(_model.Id,
            It.Is<UpdatedOrganisation>(usi =>
                usi.Type == OrganisationUpdateType.Address &&
                usi.Organisation.Addresses.First().Type == AddressType.Postal &&
                usi.Organisation.Addresses.First().StreetAddress == "1 London Street" &&
                usi.Organisation.Addresses.First().StreetAddress2 == "" &&
                usi.Organisation.Addresses.First().Region == "South" &&
                usi.Organisation.Addresses.First().PostalCode == "L1" &&
                usi.Organisation.Addresses.First().Locality == "London" &&
                usi.Organisation.Addresses.First().CountryName == "United Kingdom")), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_SameAsRegiseterdAddressIsFalse_ShouldRedirectToAddressTypeQuestion()
    {
        _model.SameAsRegiseterdAddress = false;

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierAddress");
    }
}