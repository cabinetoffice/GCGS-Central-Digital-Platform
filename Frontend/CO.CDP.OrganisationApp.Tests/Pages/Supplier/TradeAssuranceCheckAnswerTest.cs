using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class TradeAssuranceCheckAnswerTest
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly TradeAssuranceCheckAnswerModel _model;

    public TradeAssuranceCheckAnswerTest()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _mockTempDataService = new Mock<ITempDataService>();
        _model = new TradeAssuranceCheckAnswerModel(_mockOrganisationClient.Object, _mockTempDataService.Object);
    }

    [Fact]
    public void OnGet_ShouldRedirectToTradeAssuranceBody_WhenTradeAssuranceIsInvalid()
    {
        SetupTradeAssurance(new TradeAssurance());
        _model.Id = Guid.NewGuid();

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceBody");
    }

    [Fact]
    public void OnGet_ShouldReturnPage_WhenTradeAssuranceIsValid()
    {
        var validTradeAssurance = new TradeAssurance
        {
            AwardedByPersonOrBodyName = "Awarding Body",
            ReferenceNumber = "12345",
            DateAwarded = DateTime.Today
        };
        SetupTradeAssurance(validTradeAssurance);

        var result = _model.OnGet();

        _model.TradeAssurance.Should().Be(validTradeAssurance);
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
    public async Task OnPost_ShouldRedirectToTradeAssuranceBody_WhenTradeAssuranceIsInvalid()
    {
        var invalidTradeAssurance = new TradeAssurance();
        SetupTradeAssurance(invalidTradeAssurance);
        _model.Id = Guid.NewGuid();

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceBody");
    }

    [Fact]
    public async Task OnPost_ShouldUpdateTradeAssuranceAndRedirect_WhenTradeAssuranceIsValid()
    {
        var validTradeAssurance = new TradeAssurance
        {
            Id = Guid.NewGuid(),
            AwardedByPersonOrBodyName = "Awarding Body",
            ReferenceNumber = "12345",
            DateAwarded = DateTime.Today
        };
        SetupTradeAssurance(validTradeAssurance);
        _model.Id = Guid.NewGuid();

        var result = await _model.OnPost();

        _mockOrganisationClient.Verify(o => o.UpdateSupplierInformationAsync(_model.Id,
            It.Is<WebApiClient.UpdateSupplierInformation>(usi =>
                usi.Type == WebApiClient.SupplierInformationUpdateType.TradeAssurance &&
                usi.SupplierInformation.TradeAssurance.Id == validTradeAssurance.Id &&
                usi.SupplierInformation.TradeAssurance.AwardedByPersonOrBodyName == validTradeAssurance.AwardedByPersonOrBodyName &&
                usi.SupplierInformation.TradeAssurance.ReferenceNumber == validTradeAssurance.ReferenceNumber &&
                usi.SupplierInformation.TradeAssurance.DateAwarded == validTradeAssurance.DateAwarded)), Times.Once);

        _mockTempDataService.Verify(t => t.Remove(TradeAssurance.TempDataKey), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceSummary");
    }

    private void SetupTradeAssurance(TradeAssurance tradeAssurance)
    {
        _mockTempDataService.Setup(t => t.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);
        _mockTempDataService.Setup(t => t.GetOrDefault<TradeAssurance>(TradeAssurance.TempDataKey)).Returns(tradeAssurance);
    }
}