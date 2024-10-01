using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityQuestionTest
{
    private readonly ConnectedEntitySupplierHasControlModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ConnectedEntitySupplierHasControlModel(_mockOrganisationClient.Object, _sessionMock.Object);
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

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId
        };

        return connectedPersonDetails;
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenApiExceptionIsThrown()
    {
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        _mockOrganisationClient.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        var result = await _model.OnGet(true);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPageResult_WhenSessionStateIsInvalid()
    {
        _model.Id = Guid.NewGuid();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns((ConnectedEntityState?)null);

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSupplierInformationSummaryPageWithId()
    {
        _model.ControlledByPersonOrCompany = false;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
           .ReturnsAsync(OrganisationClientModel(_model.Id));

        _mockOrganisationClient.Setup(client => client.GetConnectedEntitiesAsync(_model.Id))
           .ReturnsAsync(ConnectedEntities);

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        _mockOrganisationClient.Verify(c => c.GetConnectedEntitiesAsync(_model.Id), Times.Once);

        redirectToPageResult.PageName.Should().Be("/Supplier/SupplierInformationSummary");

    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConnectedEntitySupplierCompanyQuestion()
    {
        _model.ControlledByPersonOrCompany = true;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");

    }

    [Fact]
    public async Task OnPost_ShouldUpdateSupplierCompletedConnectedPerson_WhenNoConnectedEntitiesExist()
    {
        _model.ControlledByPersonOrCompany = false;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
             Returns(DummyConnectedPersonDetails());

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
         new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e1", It.IsAny<Uri>()),
        new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e2", It.IsAny<Uri>()),
    ];

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            null,
            contactPoint: new ContactPoint(email: "test@test.com", name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            roles: [PartyRole.Supplier]);
}