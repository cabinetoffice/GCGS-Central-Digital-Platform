using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using LegalForm = CO.CDP.OrganisationApp.Pages.Supplier.LegalForm;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormSelectOrganisationModelTests
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormSelectOrganisationModel _model;
    private readonly Mock<CO.CDP.Organisation.WebApiClient.IOrganisationClient> _mockOrganisationClient;

    public LegalFormSelectOrganisationModelTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _mockOrganisationClient = new Mock<CO.CDP.Organisation.WebApiClient.IOrganisationClient>();
        _model = new LegalFormSelectOrganisationModel(_mockTempDataService.Object, _mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_SetsRegisteredOrg_FromTempData()
    {
        var id = Guid.NewGuid();
        var legalForm = new LegalForm
        {
            RegisteredLegalForm = "LimitedCompany"
        };
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet(id);

        _model.RegisteredOrg.Should().Be("LimitedCompany");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_WithValidId_CallsGetOrganisationAsync()
    {
        _model.Id = Guid.NewGuid();
        var legalForm = new LegalForm
        {
            RegisteredLegalForm = "LimitedCompany"
        };

        _mockOrganisationClient.Setup(o => o.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(GivenOrganisationClientModel(_model.Id));

        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet(_model.Id);

        _mockOrganisationClient.Verify(c => c.GetOrganisationAsync(_model.Id), Times.Once);
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFound()
    {
        var id = Guid.NewGuid();
        _mockOrganisationClient.Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
             .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public void OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {

        _model.ModelState.AddModelError("RegisteredOrg", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_UpdatesRegisteredOrgInTempData_AndRedirects()
    {

        var id = Guid.NewGuid();
        _model.Id = id;
        _model.RegisteredOrg = "LLP";
        var legalForm = new LegalForm();
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = _model.OnPost();

        legalForm.RegisteredLegalForm.Should().Be("LLP");
        _mockTempDataService.Verify(s => s.Put(LegalForm.TempDataKey, legalForm), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormLawRegistered");
    }

    [Fact]
    public void OrganisationLegalForm_ShouldReturnCorrectDictionary()
    {
        var orgLegalForm = LegalFormSelectOrganisationModel.OrganisationLegalForm;

        orgLegalForm.Should().ContainKey("LimitedCompany").WhoseValue.Should().Be("Limited company");
        orgLegalForm.Should().ContainKey("LLP").WhoseValue.Should().Be("Limited liability partnership (LLP)");
        orgLegalForm.Should().ContainKey("LimitedPartnership").WhoseValue.Should().Be("Limited partnership");
        orgLegalForm.Should().ContainKey("Other").WhoseValue.Should().Be("Other");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, approvedOn: null, contactPoint: null, id: id ?? Guid.NewGuid(), identifier: null, name: "Test Org", roles: [], details: new Details(approval: null, pendingRoles: []));
    }
}