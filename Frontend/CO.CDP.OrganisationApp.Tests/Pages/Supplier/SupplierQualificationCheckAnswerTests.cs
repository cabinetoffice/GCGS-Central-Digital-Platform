using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierQualificationCheckAnswerTests
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly SupplierQualificationCheckAnswerModel _model;

    public SupplierQualificationCheckAnswerTests()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new SupplierQualificationCheckAnswerModel(_mockOrganisationClient.Object, _mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldRedirectToQualificationBody_WhenQualificationIsInvalid()
    {
        SetupQualification(new Qualification());
        _model.Id = Guid.NewGuid();

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationAwardingBody");
    }

    [Fact]
    public void OnGet_ShouldReturnPage_WhenQualificationIsValid()
    {
        var validQualification = new Qualification
        {
            AwardedByPersonOrBodyName = "Awarding Body",
            Name = "BG",
            DateAwarded = DateTime.Today
        };
        SetupQualification(validQualification);

        var result = _model.OnGet();

        _model.Qualification.Should().Be(validQualification);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToQualificationBody_WhenQualificationIsInvalid()
    {
        var invalidQualification = new Qualification();
        SetupQualification(invalidQualification);
        _model.Id = Guid.NewGuid();

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationAwardingBody");
    }

    [Fact]
    public async Task OnPost_ShouldUpdateQualificationAndRedirect_WhenQualificationIsValid()
    {
        var validQualification = new Qualification
        {
            Id = Guid.NewGuid(),
            AwardedByPersonOrBodyName = "Awarding Body",
            Name = "BG",
            DateAwarded = DateTime.Today
        };
        SetupQualification(validQualification);
        _model.Id = Guid.NewGuid();

        var result = await _model.OnPost();

        _mockOrganisationClient.Verify(o => o.UpdateSupplierInformationAsync(_model.Id,
            It.Is<WebApiClient.UpdateSupplierInformation>(usi =>
                usi.Type == WebApiClient.SupplierInformationUpdateType.Qualification &&
                usi.SupplierInformation.Qualification.Id == validQualification.Id &&
                usi.SupplierInformation.Qualification.AwardedByPersonOrBodyName == validQualification.AwardedByPersonOrBodyName &&
                usi.SupplierInformation.Qualification.Name == validQualification.Name &&
                usi.SupplierInformation.Qualification.DateAwarded == validQualification.DateAwarded)), Times.Once);

        _mockTempDataService.Verify(t => t.Remove(Qualification.TempDataKey), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationSummary");
    }

    private void SetupQualification(Qualification qualification)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<Qualification>(Qualification.TempDataKey)).Returns(qualification);
        _mockTempDataService.Setup(t => t.GetOrDefault<Qualification>(Qualification.TempDataKey)).Returns(qualification);
    }
}