using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierIndividualOrOrgTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierIndividualOrOrgModel _model;
    private readonly Guid _organisationId;

    public SupplierIndividualOrOrgTest()
    {
        _organisationId = Guid.NewGuid();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierIndividualOrOrgModel(_organisationClientMock.Object)
        {
            Id = _organisationId
        };
    }

    [Fact]
    public async Task OnGet_SetSupplierInformationAndReturnPage()
    {
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();

        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(_organisationId))
            .ReturnsAsync(supplierInformation);

        var result = await _model.OnGet(_organisationId);

        _model.SupplierType.Should().Be(SupplierType.Organisation);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(_organisationId))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(_organisationId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPage()
    {
        _model.ModelState.AddModelError("error", "some error");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ValidModelState_UpdatesSupplierInformationAndRedirects()
    {
        _model.SupplierType = SupplierType.Organisation;

        var result = await _model.OnPost();

        _organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(_organisationId,
            It.Is<UpdateSupplierInformation>(u => u.SupplierInformation.SupplierType == SupplierType.Organisation)), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");

        (result as RedirectToPageResult)?.RouteValues?.GetValueOrDefault("Id").Should().Be(_organisationId);
    }

    [Fact]
    public async Task OnPost_ValidModelState_ThrowsApiException_ShouldRedirectToPageNotFound()
    {
        _model.SupplierType = SupplierType.Organisation;

        _organisationClientMock.Setup(o => o.UpdateSupplierInformationAsync(_organisationId,
            It.IsAny<UpdateSupplierInformation>())).ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

}