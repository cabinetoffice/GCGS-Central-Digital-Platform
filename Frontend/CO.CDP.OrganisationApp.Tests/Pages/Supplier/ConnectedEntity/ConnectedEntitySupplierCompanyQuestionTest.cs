using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntitySupplierCompanyQuestionTest
{
    private readonly ConnectedEntitySupplierCompanyQuestionModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntitySupplierCompanyQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntitySupplierCompanyQuestionModel(_sessionMock.Object);
        _model.Id = Guid.NewGuid();
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(DummyConnectedPersonDetails());

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntityQuestion_WhenSessionStateIsNull()
    {
        _model.Id = Guid.NewGuid();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns((ConnectedEntityState?)null);

        var result = _model.OnGet(null);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierHasControl");
    }

    [Fact]
    public void OnGet_ShouldSetConnectedEntityType_WhenSessionStateIsNotNull()
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldRedirectToConnectedEntitySupplierHasControl_WhenModelStateIsInvalid()
    {
        _model.RegisteredWithCh = null;
        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierHasControl");
    }

    [Fact]
    public void OnPost_ShouldRedirectToConnectedEntityPersonType()
    {
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(DummyConnectedPersonDetails());

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntityPersonType");

    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Organisation
        };

        return connectedPersonDetails;
    }

    private static List<ConnectedEntityLookup> ConnectedEntities =>
    [
         new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e1", It.IsAny<Uri>()),
        new(Guid.NewGuid(), ConnectedEntityType.Organisation, "e2", It.IsAny<Uri>()),
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