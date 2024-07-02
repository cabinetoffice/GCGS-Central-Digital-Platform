using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormOtherOrganisationModelTests
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormOtherOrganisationModel _model;
    private readonly Mock<Organisation.WebApiClient.IOrganisationClient> _mockOrganisationClient;

    public LegalFormOtherOrganisationModelTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _mockOrganisationClient = new Mock<Organisation.WebApiClient.IOrganisationClient>();
        _model = new LegalFormOtherOrganisationModel(_mockTempDataService.Object, _mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _model.Id = Guid.NewGuid();

        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(_model.Id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_SetsOtherOrganisation_FromTempData()
    {
        var id = Guid.NewGuid();
        var legalForm = new LegalForm
        {
            RegisteredLegalForm = "Some Legal Form"
        };
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet(id);

        _model.OtherOrganisation.Should().Be("Some Legal Form");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("OtherOrganisation", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_UpdatesOtherOrganisationInTempData_AndRedirects()
    {

        var id = Guid.NewGuid();
        _model.Id = id;
        _model.OtherOrganisation = "Some Legal Form";
        var legalForm = new LegalForm();
        _mockTempDataService.Setup(s => s.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = _model.OnPost();

        legalForm.RegisteredLegalForm.Should().Be("Some Legal Form");
        _mockTempDataService.Verify(s => s.Put(LegalForm.TempDataKey, legalForm), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormLawRegistered");
    }
}