using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierQualificationSummaryTests
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly SupplierQualificationSummaryModel _model;

    public SupplierQualificationSummaryTests()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new SupplierQualificationSummaryModel(_mockOrganisationClient.Object, _mockTempDataService.Object)
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
        _model.HasQualification.Should().BeTrue();
        _model.Qualifications.Should().BeEmpty();
        _model.CompletedQualification.Should().BeFalse();
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
    public async Task OnGetChange_ReturnsRedirectToPage_WhenQualificationNotFound()
    {
        SetupOrganisationClientMock(true);

        var result = await _model.OnGetChange(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_UpdatesQualification_WhenNoQualificationSelectedAndNotCompleted()
    {
        _model.HasQualification = false;
        SetupOrganisationClientMock();

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
        _mockOrganisationClient.Verify(x => x.UpdateSupplierInformationAsync(_model.Id, It.IsAny<WebApiClient.UpdateSupplierInformation>()), Times.Once);
        _mockTempDataService.Verify(x => x.Put(Qualification.TempDataKey, It.IsAny<Qualification>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_SavesTempData_WhenHasQualificationIsTrue()
    {
        _model.HasQualification = true;
        SetupOrganisationClientMock(true);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationAwardingBody");
        _mockTempDataService.Verify(x => x.Put(Qualification.TempDataKey, It.IsAny<Qualification>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        SetupOrganisationClientMock();
        _model.ModelState.AddModelError("HasQualification", "The HasQualification field is required");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _mockOrganisationClient.Verify(x => x.GetOrganisationSupplierInformationAsync(_model.Id), Times.Once);
        _mockOrganisationClient.Verify(x => x.UpdateSupplierInformationAsync(_model.Id, It.IsAny<WebApiClient.UpdateSupplierInformation>()), Times.Never);
        _mockTempDataService.Verify(x => x.Put(Qualification.TempDataKey, It.IsAny<Qualification>()), Times.Never);
    }

    private void SetupOrganisationClientMock(bool completedQualification = false)
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel(completedQualification);
        _mockOrganisationClient.Setup(x => x.GetOrganisationSupplierInformationAsync(_model.Id)).ReturnsAsync(supplierInfo);
    }
}