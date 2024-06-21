using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class TradeAssuranceSummaryTest
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly TradeAssuranceSummaryModel _model;

    public TradeAssuranceSummaryTest()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new TradeAssuranceSummaryModel(_mockOrganisationClient.Object, _mockTempDataService.Object)
        {
            Id = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WithSupplierInfo()
    {
        SetupOrganisationClientMock(true);

        var result = await _model.OnGet(true);

        result.Should().BeOfType<PageResult>();
        _model.HasTradeAssurance.Should().BeTrue();
        _model.TradeAssurances.Should().BeEmpty();
        _model.CompletedTradeAssurance.Should().BeTrue();
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFound()
    {
        _mockOrganisationClient.Setup(x => x.GetOrganisationSupplierInformationAsync(_model.Id))
            .ThrowsAsync(new WebApiClient.ApiException("", 404, "", default, null));

        var result = await _model.OnGet(true);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetChange_ReturnsRedirectToPage_WhenTradeAssuranceNotFound()
    {
        SetupOrganisationClientMock(true);

        var result = await _model.OnGetChange(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_UpdatesTradeAssurance_WhenNoTradeAssuranceSelectedAndNotCompleted()
    {
        _model.HasTradeAssurance = false;
        SetupOrganisationClientMock();

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
        _mockOrganisationClient.Verify(x => x.UpdateSupplierInformationAsync(_model.Id, It.IsAny<WebApiClient.UpdateSupplierInformation>()), Times.Once);
        _mockTempDataService.Verify(x => x.Put(TradeAssurance.TempDataKey, It.IsAny<TradeAssurance>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_SavesTempData_WhenHasTradeAssuranceIsTrue()
    {
        _model.HasTradeAssurance = true;
        SetupOrganisationClientMock(true);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceBody");
        _mockTempDataService.Verify(x => x.Put(TradeAssurance.TempDataKey, It.IsAny<TradeAssurance>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        SetupOrganisationClientMock();
        _model.ModelState.AddModelError("HasTradeAssurance", "The HasTradeAssurance field is required");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _mockOrganisationClient.Verify(x => x.GetOrganisationSupplierInformationAsync(_model.Id), Times.Once);
        _mockOrganisationClient.Verify(x => x.UpdateSupplierInformationAsync(_model.Id, It.IsAny<WebApiClient.UpdateSupplierInformation>()), Times.Never);
        _mockTempDataService.Verify(x => x.Put(TradeAssurance.TempDataKey, It.IsAny<TradeAssurance>()), Times.Never);
    }

    private void SetupOrganisationClientMock(bool completedTradeAssurance = false)
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel(completedTradeAssurance);
        _mockOrganisationClient.Setup(x => x.GetOrganisationSupplierInformationAsync(_model.Id)).ReturnsAsync(supplierInfo);
    }
}