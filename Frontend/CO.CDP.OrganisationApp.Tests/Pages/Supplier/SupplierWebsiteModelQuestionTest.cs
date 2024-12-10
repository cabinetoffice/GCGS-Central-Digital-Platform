using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierWebsiteModelQuestionTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierWebsiteQuestionModel _model;

    public SupplierWebsiteModelQuestionTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierWebsiteQuestionModel(_organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierInformationClientModel);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<PageResult>();
        _model.HasWebsiteAddress.Should().Be(true);
        _model.WebsiteAddress.Should().Be("https://xyz.com/");
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


    [Theory]
    [InlineData("http://xyz.com")]
    [InlineData("https://example.com")]
    [InlineData("//valid-doamain.com/test-page")]
    [InlineData("httpvalid-doamain.com")]
    public async Task OnPost_WithValidUrl_ReturnsRedirectToSupplierBasicInformation(string url)
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasWebsiteAddress = true;
        _model.WebsiteAddress = url;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.UpdateSupplierInformationAsync(id,
            It.IsAny<UpdateSupplierInformation>())).Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("HasWebsiteAddress", "Select an option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_InvalidaWebsiteUrl_ReturnsPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasWebsiteAddress = true;
        _model.WebsiteAddress = "https://.invalid-doamain.com/test-page";

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Never);
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasWebsiteAddress = false;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    private static SupplierInformation SupplierInformationClientModel => new(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: false,
            completedWebsiteAddress: true,
            completedEmailAddress: false,
            completedOperationType: false,
            completedLegalForm: false,
            completedConnectedPerson: false,
            legalForm: null);

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            contactPoint: new ContactPoint(email: null, name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier],
            details: new Details(approval: null, pendingRoles: [])
        );
}