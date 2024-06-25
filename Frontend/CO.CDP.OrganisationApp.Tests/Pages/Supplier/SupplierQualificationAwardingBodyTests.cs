using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierQualificationAwardingBodyTests
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly SupplierQualificationAwardingBodyModel _model;

    public SupplierQualificationAwardingBodyTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new SupplierQualificationAwardingBodyModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldPopulateFields_WhenQualificationIsPresent()
    {
        var qualification = new Qualification
        {
            Id = Guid.NewGuid(),
            AwardedByPersonOrBodyName = "Awarding Body"
        };
        SetupQualification(qualification);

        var result = _model.OnGet();

        _model.QualificationId.Should().Be(qualification.Id);
        _model.AwardedByPersonOrBodyName.Should().Be("Awarding Body");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnGet_ShouldNotPopulateFields_WhenQualificationIsNotPresent()
    {
        var qualification = new Qualification();
        SetupQualification(qualification);

        var result = _model.OnGet();

        _model.QualificationId.Should().Be(qualification.Id);
        _model.AwardedByPersonOrBodyName.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenQualificationModelStateIsInvalid()
    {
        _model.AwardedByPersonOrBodyName = null;
        _model.ModelState.AddModelError("AwardedByPersonOrBodyName", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_ShouldRedirect_WhenQualificationModelStateIsValid()
    {
        _model.AwardedByPersonOrBodyName = "Awarding Body";
        _model.Id = Guid.NewGuid();
        SetupQualification(new Qualification());

        var result = _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(Qualification.TempDataKey,
            It.Is<Qualification>(ta => ta.AwardedByPersonOrBodyName == "Awarding Body")), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationAwardedDate");
    }

    private void SetupQualification(Qualification qualification)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<Qualification>(Qualification.TempDataKey)).Returns(qualification);
    }
}