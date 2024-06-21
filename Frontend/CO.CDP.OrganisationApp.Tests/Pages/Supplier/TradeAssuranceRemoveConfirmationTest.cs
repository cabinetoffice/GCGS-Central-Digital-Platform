using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class TradeAssuranceRemoveConfirmationTest
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly TradeAssuranceRemoveConfirmationModel _model;

    public TradeAssuranceRemoveConfirmationTest()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _model = new TradeAssuranceRemoveConfirmationModel(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WhenTradeAssuranceExists()
    {
        var tradeAssuranceId = Guid.NewGuid();
        _model.Id = Guid.NewGuid();
        _model.TradeAssuranceId = tradeAssuranceId;
        SetupOrganisationClientMock(tradeAssuranceId);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ReturnsNotFoundResult_WhenTradeAssuranceDoesNotExist()
    {
        _model.Id = Guid.NewGuid();
        _model.TradeAssuranceId = Guid.NewGuid();
        _mockOrganisationClient.Setup(o => o.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("ConfirmRemove", "Please confirm remove trade assurance option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_DeletesTradeAssuranceAndRedirects_WhenConfirmationIsTrue()
    {
        var tradeAssuranceId = Guid.NewGuid();
        _model.Id = Guid.NewGuid();
        _model.TradeAssuranceId = tradeAssuranceId;
        _model.ConfirmRemove = true;
        SetupOrganisationClientMock(tradeAssuranceId);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("TradeAssuranceSummary");

        _mockOrganisationClient.Verify(o => o.DeleteSupplierInformationAsync(_model.Id,
            It.Is<WebApiClient.DeleteSupplierInformation>(usi =>
                usi.Type == WebApiClient.SupplierInformationDeleteType.TradeAssurance &&
                usi.TradeAssuranceId == tradeAssuranceId)), Times.Once);
    }

    private void SetupOrganisationClientMock(Guid tradeAssuranceId)
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        supplierInfo.TradeAssurances.Add(new WebApiClient.TradeAssurance("Awarding Body", new DateTime(2023, 6, 15), tradeAssuranceId, "654321"));
        _mockOrganisationClient.Setup(o => o.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(supplierInfo);
    }
}