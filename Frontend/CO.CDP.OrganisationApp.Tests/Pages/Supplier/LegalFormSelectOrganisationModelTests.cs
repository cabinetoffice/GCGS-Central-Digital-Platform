using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormSelectOrganisationModelTests
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormSelectOrganisationModel _model;

    public LegalFormSelectOrganisationModelTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new LegalFormSelectOrganisationModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_SetsRegisteredOrg_FromTempData()
    {
        var id = Guid.NewGuid();
        var legalForm = new LegalForm
        {
            RegisteredLegalForm = "LimitedCompany"
        };
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = _model.OnGet(id);

        _model.RegisteredOrg.Should().Be("LimitedCompany");
        result.Should().BeOfType<PageResult>();
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
}