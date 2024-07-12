using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedQuestionTest
{
    private readonly ConnectedQuestionModel _model;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;

    public ConnectedQuestionTest()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ConnectedQuestionModel(_mockOrganisationClient.Object, _mockTempDataService.Object);
        _model.Id = Guid.NewGuid();
    }

    [Fact]
    public async Task OnGet_ShouldReturnPageResult()
    {
        var result = await _model.OnGet(null);

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
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFound()
    {
        _mockOrganisationClient.Setup(x => x.GetOrganisationSupplierInformationAsync(_model.Id))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        var result = await _model.OnGet(true);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSupplierInformationSummaryPageWithId()
    {
        _model.ControlledByPersonOrCompany = false;
        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
           .ReturnsAsync(OrganisationClientModel(_model.Id));

        _mockOrganisationClient.Setup(client => client.GetConnectedEntitiesAsync(_model.Id))
           .ReturnsAsync(ConnectedEntities);

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Supplier/SupplierInformationSummary");

    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConnectedCompaniesQuestionPageWithId()
    {
        _model.ControlledByPersonOrCompany = true;
        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedCompaniesQuestion");

    }

    [Fact]
    public async Task OnPost_ShouldUpdateSupplierCompletedConnectedPerson_WhenNoConnectedEntitiesExist()
    {
        _model.ControlledByPersonOrCompany = false;
       
        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
           .ReturnsAsync(OrganisationClientModel(_model.Id));
                
        _mockOrganisationClient.Setup(client => client.GetConnectedEntitiesAsync(_model.Id))
           .ReturnsAsync(new List<ConnectedEntityLookup>());

        _mockOrganisationClient.Setup(client => client.UpdateSupplierInformationAsync(_model.Id,
            It.IsAny<UpdateSupplierInformation>())).Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        _mockOrganisationClient.Verify(c => c.UpdateSupplierInformationAsync(_model.Id, It.IsAny<UpdateSupplierInformation>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Supplier/SupplierInformationSummary");
    }

    private static List<ConnectedEntityLookup> ConnectedEntities =>
    [
         new(Guid.NewGuid(), "e1",It.IsAny<Uri>()),
         new(Guid.NewGuid(), "e2",It.IsAny<Uri>()),
    ];

    private static SupplierInformation SupplierInformationClientModel => new(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: false,
            completedWebsiteAddress: false,
            completedEmailAddress: true,
            completedQualification: false,
            completedTradeAssurance: false,
            completedOperationType: false,
            completedLegalForm: false,
            completedConnectedPerson: false,
            tradeAssurances: null,
            legalForm: null,
            qualifications: null);

    private static Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            contactPoint: new ContactPoint(email: "test@test.com", faxNumber: null, name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            roles: [PartyRole.Supplier]);
}