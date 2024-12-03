using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerSelectDevolvedRegulationTest
{
    private readonly Mock<ITempDataService> tempDataServiceMock;
    private readonly SupplierToBuyerSelectDevolvedRegulationModel _model;
    private readonly Guid orgGuid = Guid.NewGuid();

    public SupplierToBuyerSelectDevolvedRegulationTest()
    {
        tempDataServiceMock = new Mock<ITempDataService>();
        _model = new SupplierToBuyerSelectDevolvedRegulationModel(tempDataServiceMock.Object)
        {
            Id = orgGuid,
            Regulations = new List<DevolvedRegulation>()
        };
    }

    [Fact]
    public void OnGet_ShouldLoadRegulationsFromTempData()
    {
        var state = new SupplierToBuyerDetails
        {
            Regulations = new List<DevolvedRegulation> { DevolvedRegulation.Scotland, DevolvedRegulation.Wales }
        };
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Regulations.Should().BeEquivalentTo(new List<DevolvedRegulation> { DevolvedRegulation.Scotland, DevolvedRegulation.Wales });
    }

    [Fact]
    public void OnGet_ShouldSetRegulationsToEmpty_WhenStateIsNotAvailable()
    {
        tempDataServiceMock
         .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
         .Returns(new SupplierToBuyerDetails());

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Regulations.Should().BeEmpty();
    }

    [Fact]
    public void OnPost_ShouldUpdateStateAndRedirectToSummary()
    {
        _model.Regulations = new List<DevolvedRegulation> { DevolvedRegulation.Scotland };

        var state = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnPost();

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s => s.Regulations.SequenceEqual(new List<DevolvedRegulation> { DevolvedRegulation.Scotland }))), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationDetailsSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Regulations", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }
}
