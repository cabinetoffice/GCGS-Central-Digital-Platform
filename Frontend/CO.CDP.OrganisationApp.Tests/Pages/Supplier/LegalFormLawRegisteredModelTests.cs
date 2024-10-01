using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using LegalForm = CO.CDP.OrganisationApp.Pages.Supplier.LegalForm;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormLawRegisteredModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormLawRegisteredModel _model;

    public LegalFormLawRegisteredModelTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new LegalFormLawRegisteredModel(_mockTempDataService.Object, _mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_SetsLawRegisteredAndRegisteredUnderAct2006_FromTempData()
    {
        var id = Guid.NewGuid();

        var legalForm = new LegalForm
        {
            LawRegistered = "Some Law",
            RegisteredUnderAct2006 = true
        };

        _mockOrganisationClient.Setup(o => o.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(GivenOrganisationClientModel(_model.Id));

        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet(id);

        _model.LawRegistered.Should().Be("Some Law");
        _model.RegisteredUnderAct2006.Should().BeTrue();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _model.Id = Guid.NewGuid();

        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(_model.Id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public void OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("LawRegistered", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_UpdatesLawRegisteredInTempData_AndRedirects()
    {

        var id = Guid.NewGuid();
        _model.Id = id;
        _model.LawRegistered = "Some Law";
        var legalForm = new LegalForm();
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = _model.OnPost();

        legalForm.LawRegistered.Should().Be("Some Law");
        _mockTempDataService.Verify(s => s.Put(LegalForm.TempDataKey, legalForm), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormFormationDate");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, id!.Value, null, "Test Org", []);
    }
}