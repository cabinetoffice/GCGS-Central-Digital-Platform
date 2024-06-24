using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using LegalForm = CO.CDP.OrganisationApp.Pages.Supplier.LegalForm;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormCaQuestionModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormCaQuestionModel _model;

    public LegalFormCaQuestionModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new LegalFormCaQuestionModel(_mockOrganisationClient.Object, _mockTempDataService.Object);
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
        _model.RegisteredOnCh = true;
        var result = await _model.OnGet(_model.Id);


        _model.RegisteredOnCh.Should().BeTrue();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenApiExceptionIsThrown()
    {

        var id = Guid.NewGuid();
        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));


        var result = await _model.OnGet(id);


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
        _model.RegisteredOnCh = true;
        _model.Id = id;

        var legalForm = new CO.CDP.Organisation.WebApiClient.LegalForm("LawRegistered", "RegisteredLegalForm", true, new DateTimeOffset());
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
        _model.RegisteredOnCh = false;
        _model.Id = id;
        var legalForm = new CO.CDP.Organisation.WebApiClient.LegalForm("LawRegistered", "RegisteredLegalForm", true, new DateTimeOffset());
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
        _model.RegisteredOnCh = true;
        _model.Id = id;
        _mockOrganisationClient.Setup(c => c.GetOrganisationSupplierInformationAsync(id))
                               .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));


        var result = await _model.OnPost();


        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}