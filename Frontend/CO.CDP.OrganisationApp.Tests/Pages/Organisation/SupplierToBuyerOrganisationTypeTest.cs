using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerOrganisationTypeTest
{
    private readonly Mock<ITempDataService> tempDataServiceMock;
    private readonly SupplierToBuyerOrganisationTypeModel _model;
    private readonly Guid orgGuid = Guid.NewGuid();

    public SupplierToBuyerOrganisationTypeTest()
    {
        tempDataServiceMock = new Mock<ITempDataService>();
        _model = new SupplierToBuyerOrganisationTypeModel(tempDataServiceMock.Object)
        {
            Id = orgGuid
        };
    }

    [Fact]
    public void OnGet_ShouldLoadStateFromTempData()
    {
        var state = new SupplierToBuyerDetails
        {
            BuyerOrganisationType = "CentralGovernment"
        };
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.BuyerOrganisationType.Should().Be("CentralGovernment");
        _model.OtherValue.Should().BeNull();
    }

    [Fact]
    public void OnGet_ShouldSetOtherValue_WhenBuyerOrganisationTypeIsUnknown()
    {
        var state = new SupplierToBuyerDetails
        {
            BuyerOrganisationType = "UnknownType"
        };
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.BuyerOrganisationType.Should().Be("Other");
        _model.OtherValue.Should().Be("UnknownType");
    }

    [Fact]
    public void OnPost_ShouldUpdateStateAndRedirectToSummary_WhenRedirectToSummaryIsTrue()
    {
        _model.BuyerOrganisationType = "CentralGovernment";
        _model.RedirectToSummary = true;

        var state = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnPost();

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s =>
            s.BuyerOrganisationType == "CentralGovernment")), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationDetailsSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public void OnPost_ShouldUpdateStateAndRedirectToDevolvedRegulation_WhenRedirectToSummaryIsFalse()
    {
        _model.BuyerOrganisationType = "PublicUndertaking";
        _model.RedirectToSummary = false;

        var state = new SupplierToBuyerDetails();
        tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        var result = _model.OnPost();

        tempDataServiceMock.Verify(t => t.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s =>
            s.BuyerOrganisationType == "PublicUndertaking")), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerDevolvedRegulation");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgGuid);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("BuyerOrganisationType", "Required");

        var result = _model.OnPost();
        result.Should().BeOfType<PageResult>();
    }
}
