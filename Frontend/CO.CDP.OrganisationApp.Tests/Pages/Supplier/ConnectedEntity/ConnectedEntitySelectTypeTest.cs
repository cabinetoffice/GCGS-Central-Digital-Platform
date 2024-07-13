using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntitySelectTypeTest
{
    private readonly ConnectedEntitySelectTypeModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntitySelectTypeTest()
    {
        _sessionMock = new Mock<ISession>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ConnectedEntitySelectTypeModel(_mockOrganisationClient.Object, _sessionMock.Object);
        _model.Id = Guid.NewGuid();
    }

    [Fact]
    public async Task OnGet_ShouldReturnPageResult()
    {
        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        
        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    private ConnectedPerson DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedPerson
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierInformationOrganisationId = _organisationId
        };

        return connectedPersonDetails;
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFoundWithOutConnectedEntityId()
    {
        _model.ConnectedEntityId = null;
        _sessionMock.Setup(s => s.Get<ConnectedPerson>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        _mockOrganisationClient.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFoundWithConnectedEntityId()
    {
        _model.ConnectedEntityId = _entityId;
        _sessionMock.Setup(s => s.Get<ConnectedPerson>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        _mockOrganisationClient.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConnectedEntitySelectCategoryPage()
    {
        _model.ConnectedEntityType = Constants.ConnectedEntityType.Organisation;
        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
           .ReturnsAsync(OrganisationClientModel(_model.Id));

        _mockOrganisationClient.Setup(client => client.GetConnectedEntitiesAsync(_model.Id))
           .ReturnsAsync(ConnectedEntities);

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySelectCategory");

    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConnectedEntitySelectCategory()
    {
        _model.ConnectedEntityType = Constants.ConnectedEntityType.Organisation;
        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySelectCategory");

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