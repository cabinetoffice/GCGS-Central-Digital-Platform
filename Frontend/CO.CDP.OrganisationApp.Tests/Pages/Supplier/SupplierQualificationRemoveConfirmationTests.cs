using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class SupplierQualificationRemoveConfirmationTests
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly SupplierQualificationRemoveConfirmationModel _model;

    public SupplierQualificationRemoveConfirmationTests()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _model = new SupplierQualificationRemoveConfirmationModel(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WhenQualificationExists()
    {
        var qualificationId = Guid.NewGuid();
        _model.Id = Guid.NewGuid();
        _model.QualificationId = qualificationId;
        SetupOrganisationClientMock(qualificationId);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ReturnsNotFoundResult_WhenQualificationDoesNotExist()
    {
        _model.Id = Guid.NewGuid();
        _model.QualificationId = Guid.NewGuid();
        _mockOrganisationClient.Setup(o => o.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("ConfirmRemove", "Please confirm remove qualification option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_DeletesQualificationAndRedirects_WhenConfirmationIsTrue()
    {
        var qualificationId = Guid.NewGuid();
        _model.Id = Guid.NewGuid();
        _model.QualificationId = qualificationId;
        _model.ConfirmRemove = true;
        SetupOrganisationClientMock(qualificationId);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierQualificationSummary");

        _mockOrganisationClient.Verify(o => o.DeleteSupplierInformationAsync(_model.Id,
            It.Is<WebApiClient.DeleteSupplierInformation>(usi =>
                usi.Type == WebApiClient.SupplierInformationDeleteType.Qualification &&
                usi.QualificationId == qualificationId)), Times.Once);
    }

    private void SetupOrganisationClientMock(Guid qualificationId)
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        supplierInfo.Qualifications.Add(new WebApiClient.Qualification("Awarding Body", new DateTime(2023, 6, 15), qualificationId, "654321"));
        _mockOrganisationClient.Setup(o => o.GetOrganisationSupplierInformationAsync(_model.Id))
            .ReturnsAsync(supplierInfo);
    }
}
