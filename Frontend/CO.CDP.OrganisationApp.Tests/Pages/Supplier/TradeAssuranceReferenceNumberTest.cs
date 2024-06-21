using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class TradeAssuranceReferenceNumberTest
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly TradeAssuranceReferenceNumberModel _model;

    public TradeAssuranceReferenceNumberTest()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new TradeAssuranceReferenceNumberModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldPopulateReferenceNumber_WhenTradeAssuranceIsPresent()
    {
        var tradeAssurance = new TradeAssurance { ReferenceNumber = "123456" };
        SetupTradeAssurance(tradeAssurance);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.ReferenceNumber.Should().Be(tradeAssurance.ReferenceNumber);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("ReferenceNumber", "Reference number is required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldUpdateTradeAssuranceAndRedirect_WhenModelStateIsValid()
    {
        var tradeAssurance = new TradeAssurance();
        SetupTradeAssurance(tradeAssurance);
        _model.ReferenceNumber = "654321";

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceAwardedDate");

        _mockTempDataService.Verify(t => t.Put(TradeAssurance.TempDataKey, It.Is<TradeAssurance>(ta =>
            ta.ReferenceNumber == _model.ReferenceNumber)), Times.Once);
    }

    private void SetupTradeAssurance(TradeAssurance tradeAssurance)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);
    }
}