using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerDevolvedRegulationTest
{
    private readonly Mock<ITempDataService> tempDataServiceMock;
    private readonly SupplierToBuyerDevolvedRegulationModel _model;
    private readonly Guid orgGuid = Guid.NewGuid();

    public SupplierToBuyerDevolvedRegulationTest()
    {
        tempDataServiceMock = new Mock<ITempDataService>();
        _model = new SupplierToBuyerDevolvedRegulationModel(tempDataServiceMock.Object)
        {
            Id = orgGuid
        };
    }

    [Fact]
    public void OnGet_ShouldLoadDevolvedStateFromTempData()
    {
        var state = new SupplierToBuyerDetails
        {
            Devolved = true
        };
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Devolved.Should().BeTrue();
    }

    [Fact]
    public void OnPost_ShouldUpdateStateAndRedirectToSelectDevolvedRegulation_WhenDevolvedIsTrue()
    {
        _model.Devolved = true;

        var state = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnPost();
        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s => s.Devolved == true)), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerSelectDevolvedRegulation");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public void OnPost_ShouldUpdateStateAndRedirectToSummary_WhenDevolvedIsFalse()
    {
        _model.Devolved = false;

        var state = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnPost();

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s => s.Devolved == false)), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationDetailsSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Devolved", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }
}
