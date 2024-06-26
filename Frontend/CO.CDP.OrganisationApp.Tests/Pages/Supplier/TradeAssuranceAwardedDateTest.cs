using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class TradeAssuranceAwardedDateTest
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly TradeAssuranceAwardedDateModel _model;

    public TradeAssuranceAwardedDateTest()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new TradeAssuranceAwardedDateModel(_mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldPopulateFields_WhenDateAwardedIsPresent()
    {
        var dateAwarded = new DateTime(2023, 6, 15);
        var tradeAssurance = new TradeAssurance { DateAwarded = dateAwarded };
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);

        var result = _model.OnGet();

        _model.Day.Should().Be("15");
        _model.Month.Should().Be("6");
        _model.Year.Should().Be("2023");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnGet_ShouldNotPopulateFields_WhenDateAwardedIsNotPresent()
    {
        var tradeAssurance = new TradeAssurance();
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);

        var result = _model.OnGet();

        _model.Day.Should().BeNull();
        _model.Month.Should().BeNull();
        _model.Year.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.Day = "32";
        _model.Month = "13";
        _model.Year = "2023";
        _model.ModelState.AddModelError("Day", "Invalid day");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenDateIsInvalid()
    {
        _model.Day = "31";
        _model.Month = "2";
        _model.Year = "2023";

        var result = _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.DateOfAward));
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenDateIsInTheFuture()
    {
        SetFutureDate();

        var result = _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.DateOfAward));
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldRedirect_WhenDateIsValid()
    {
        SetValidDate();
        _model.Id = Guid.NewGuid();
        var tradeAssurance = new TradeAssurance();
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);

        var result = _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(TradeAssurance.TempDataKey, It.Is<TradeAssurance>(ta => ta.DateAwarded == new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0)))), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceCheckAnswer");
    }

    private void SetValidDate()
    {
        _model.Day = "15";
        _model.Month = "6";
        _model.Year = "2023";
    }

    private void SetFutureDate()
    {
        var futureDate = DateTime.Now.AddDays(1);
        _model.Day = futureDate.Day.ToString();
        _model.Month = futureDate.Month.ToString();
        _model.Year = futureDate.Year.ToString();
    }
}