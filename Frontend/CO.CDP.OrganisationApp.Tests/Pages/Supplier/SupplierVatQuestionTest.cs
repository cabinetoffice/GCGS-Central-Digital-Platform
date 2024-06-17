using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierVatModelQuestionTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierVatQuestionModel _model;

    public SupplierVatModelQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierVatQuestionModel(_sessionMock.Object, _organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<PageResult>();
        _model.HasVatNumber.Should().Be(true);
        _model.VatNumber.Should().Be("FakeVatId");
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = "VAT12345";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.UpdateSupplierInformationAsync(id,
            It.IsAny<UpdateSupplierInformation>())).Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("HasVatNumber", "Please select an option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = false;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

}