using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using LegalForm = CO.CDP.OrganisationApp.Pages.Supplier.LegalForm;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormCompanyActQuestionModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormCompanyActQuestionModel _model;

    public LegalFormCompanyActQuestionModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new LegalFormCompanyActQuestionModel(_mockOrganisationClient.Object, _mockTempDataService.Object);
    }

    private void SetupOrganisationClientMock(bool completedLegalForm = false)
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel(completedLegalForm: completedLegalForm);
        _mockOrganisationClient.Setup(x => x.GetOrganisationSupplierInformationAsync(_model.Id)).ReturnsAsync(supplierInfo);
    }

    [Fact]
    public async Task OnGet_SetsRegisteredOnCh_WhenLegalFormIsCompleted()
    {
        SetupOrganisationClientMock();
        _model.RegisteredOnCompanyHouse = true;
        var result = await _model.OnGet(true);

        _model.RegisteredOnCompanyHouse.Should().BeTrue();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenApiExceptionIsThrown()
    {
        var id = Guid.NewGuid();
        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));
        _model.Id = id;
        var result = await _model.OnGet(true);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("RegisteredOnCh", "Required");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_RedirectsToCorrectPage_WhenRegisteredOnChIsTrue()
    {
        var id = Guid.NewGuid();
        _model.RegisteredOnCompanyHouse = true;
        _model.Id = id;

        var legalForm = new Organisation.WebApiClient.LegalForm("LawRegistered", "RegisteredLegalForm", true, new DateTimeOffset());
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel(completedLegalForm: true, legalForm: legalForm);

        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ReturnsAsync(supplierInfo);

        var result = await _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(LegalForm.TempDataKey, It.IsAny<LegalForm>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormSelectOrganisation");
    }

    [Fact]
    public async Task OnPost_RedirectsToCorrectPage_WhenRegisteredOnChIsFalse()
    {
        var id = Guid.NewGuid();
        _model.RegisteredOnCompanyHouse = false;
        _model.Id = id;
        var legalForm = new Organisation.WebApiClient.LegalForm("LawRegistered", "RegisteredLegalForm", true, new DateTimeOffset());
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel(completedLegalForm: true, legalForm: legalForm);
        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ReturnsAsync(supplierInfo);

        var result = await _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(LegalForm.TempDataKey, It.IsAny<LegalForm>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormOtherOrganisation");
    }

    [Fact]
    public async Task OnPost_RedirectsToPageNotFound_WhenApiExceptionIsThrown()
    {
        var id = Guid.NewGuid();
        _model.RegisteredOnCompanyHouse = true;
        _model.Id = id;
        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}