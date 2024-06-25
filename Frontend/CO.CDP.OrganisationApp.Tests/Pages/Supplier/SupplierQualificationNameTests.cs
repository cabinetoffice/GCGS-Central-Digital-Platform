using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierQualificationNameTests
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly SupplierQualificationNameModel _model;

    public SupplierQualificationNameTests()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new SupplierQualificationNameModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldPopulateReferenceNumber_WhenQualificationIsPresent()
    {
        var qualification = new Qualification { Name = "BG" };
        SetupQualification(qualification);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.QualificationName.Should().Be(qualification.Name);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenQualificationModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Name", "Name is required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldUpdateQualificationAndRedirect_WhenModelStateIsValid()
    {
        var qualification = new Qualification();
        SetupQualification(qualification);
        _model.QualificationName = "BG";

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationAwardedDate");

        _mockTempDataService.Verify(t => t.Put(Qualification.TempDataKey, It.Is<Qualification>(ta =>
            ta.Name == _model.QualificationName)), Times.Once);
    }

    private void SetupQualification(Qualification qualification)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<Qualification>(Qualification.TempDataKey)).Returns(qualification);
    }
}