using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class TradeAssuranceBodyTest
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly TradeAssuranceBodyModel _model;

    public TradeAssuranceBodyTest()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new TradeAssuranceBodyModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldPopulateFields_WhenTradeAssuranceIsPresent()
    {
        var tradeAssurance = new TradeAssurance
        {
            Id = Guid.NewGuid(),
            AwardedByPersonOrBodyName = "Awarding Body"
        };
        SetupTradeAssurance(tradeAssurance);

        var result = _model.OnGet();

        _model.TradeAssuranceId.Should().Be(tradeAssurance.Id);
        _model.AwardedByPersonOrBodyName.Should().Be("Awarding Body");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnGet_ShouldNotPopulateFields_WhenTradeAssuranceIsNotPresent()
    {
        var tradeAssurance = new TradeAssurance();
        SetupTradeAssurance(tradeAssurance);

        var result = _model.OnGet();

        _model.TradeAssuranceId.Should().Be(tradeAssurance.Id);
        _model.AwardedByPersonOrBodyName.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.AwardedByPersonOrBodyName = null;
        _model.ModelState.AddModelError("AwardedByPersonOrBodyName", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_ShouldRedirect_WhenModelStateIsValid()
    {
        _model.AwardedByPersonOrBodyName = "Awarding Body";
        _model.Id = Guid.NewGuid();
        SetupTradeAssurance(new TradeAssurance());

        var result = _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(TradeAssurance.TempDataKey,
            It.Is<TradeAssurance>(ta => ta.AwardedByPersonOrBodyName == "Awarding Body")), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceReferenceNumber");
    }

    private void SetupTradeAssurance(TradeAssurance tradeAssurance)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);
    }
}