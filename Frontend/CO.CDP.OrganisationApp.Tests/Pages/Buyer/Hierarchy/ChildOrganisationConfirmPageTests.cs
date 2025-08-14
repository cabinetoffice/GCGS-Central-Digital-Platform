using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using CO.CDP.Localization;
using Address = CO.CDP.Organisation.WebApiClient.Address;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationConfirmPageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ILogger<ChildOrganisationConfirmPage>> _mockLogger;
    private readonly ChildOrganisationConfirmPage _model;

    public ChildOrganisationConfirmPageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockLogger = new Mock<ILogger<ChildOrganisationConfirmPage>>();
        var mockAuthorizationService = new Mock<IAuthorizationService>();
        _model = new ChildOrganisationConfirmPage(_mockOrganisationClient.Object, _mockLogger.Object);
        mockAuthorizationService.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Success());
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIdEmpty_RedirectsToSearchPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.ChildId = Guid.Empty;

        var result = await _model.OnGetAsync();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationSearchPage");
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues?["Id"].Should().Be(id);
    }

    [Fact]
    public async Task OnGetAsync_WhenGetOrganisationThrowsException_RedirectsToErrorPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var ppon = "GB-PPON:ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Ppon = ppon;

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _model.OnGetAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred while retrieving organisation details"),
                It.Is<Exception>(e => e is CdpExceptionLogging && ((CdpExceptionLogging)e).ErrorCode == "LOOKUP_ERROR"),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenOrganisationIsNull_RedirectsToErrorPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var ppon = "GB-PPON:ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Ppon = ppon;

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync((CDP.Organisation.WebApiClient.Organisation?)null);

        var result = await _model.OnGetAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Organisation not found for ChildId")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WithValidChildId_SetsOrganisation()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string query = "test query";
        const string ppon = "GB-PPON:ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Query = query;
        _model.Ppon = ppon;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses:
            [
                new Address("Line1", "Line2", "City", "PostalCode", "Region", "StreetAddress", AddressType.Registered)
            ],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: childId,
            identifier: new Identifier("12345", "Test Org", "DUNS", new Uri("http://whatever")),
            name: "Test Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync(organisation);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.ChildOrganisation.Should().NotBeNull();
        _model.ChildOrganisation!.Name.Should().Be("Test Organisation");
        _model.ChildOrganisation.Id.Should().Be(childId);
        _model.ChildOrganisationAddress.Should().NotBeNull();
        _model.ChildOrganisationContactPoint.Should().NotBeNull();
        _model.ChildOrganisationType.Should().Be("Buyer");
    }

    [Fact]
    public async Task OnPostAsync_WhenExceptionOccurs_RedirectsToErrorPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _model.Id = id;
        _model.ChildId = childId;

        _mockOrganisationClient
            .Setup(client =>
                client.CreateParentChildRelationshipAsync(
                    id,
                    It.IsAny<CreateParentChildRelationshipRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _model.OnPostAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "Error occurred while establishing parent-child relationship"),
                It.Is<Exception>(e =>
                    e is CdpExceptionLogging && ((CdpExceptionLogging)e).ErrorCode == "RELATIONSHIP_ERROR"),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnPostAsync_WhenSuccessful_RedirectsToSuccessPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string organisationName = "Test Organisation";

        _model.Id = id;
        _model.ChildId = childId;
        _model.ChildOrganisationName = organisationName;

        var relationshipId = Guid.NewGuid();
        _mockOrganisationClient
            .Setup(client =>
                client.CreateParentChildRelationshipAsync(
                    id,
                    It.IsAny<CreateParentChildRelationshipRequest>()))
            .ReturnsAsync(new CreateParentChildRelationshipResult(relationshipId, true));

        var result = await _model.OnPostAsync();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationSuccessPage");
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues?["Id"].Should().Be(id);
        redirectResult.RouteValues.Should().ContainKey("OrganisationName");
        redirectResult.RouteValues?["OrganisationName"].Should().Be(organisationName);

        _mockOrganisationClient.Verify(
            client => client.CreateParentChildRelationshipAsync(
                id,
                It.Is<CreateParentChildRelationshipRequest>(r =>
                    r.ParentId == id &&
                    r.ChildId == childId)),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIsPendingBuyer_SetsWarningMessage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string ppon = "ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Ppon = ppon;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: null,
            id: childId,
            identifier: new Identifier("12345", "Test Org", "DUNS", new Uri("http://test")),
            name: "Test Organisation",
            type: OrganisationType.Organisation,
            roles: [],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [PartyRole.Buyer],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync(organisation);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.WarningTagMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Tag_ApprovalPending);
        _model.WarningMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Warning_ApprovalPending);
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIsConnectedAsParent_SetsWarningMessage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string ppon = "ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Ppon = ppon;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: null,
            id: childId,
            identifier: new Identifier("12345", "Test Org", "DUNS", new Uri("http://test")),
            name: "Test Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync(organisation);

        _mockOrganisationClient
            .Setup(client => client.GetParentOrganisationsAsync(id))
            .ReturnsAsync(new List<OrganisationSummary> {
                new(childId, "Test Organisation", "Buyer", null)
            });

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.WarningTagMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Tag_ChildConnectedAsParent);
        _model.WarningMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Warning_ChildConnectedAsParent);
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIsConnectedAsChild_SetsWarningMessage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string ppon = "ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = childId;
        _model.Ppon = ppon;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: null,
            id: childId,
            identifier: new Identifier("12345", "Test Org", "DUNS", new Uri("http://test")),
            name: "Test Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync(organisation);

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(id))
            .ReturnsAsync(new List<OrganisationSummary> {
                new(childId, "Test Organisation", "Buyer", null)
            });

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.WarningTagMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Tag_ChildConnectedAsChild);
        _model.WarningMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Warning_ChildConnectedAsChild);
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIsSameAsOrganisation_SetsWarningMessage()
    {
        var id = Guid.NewGuid();
        const string ppon = "ABCD-1234-EFGH";

        _model.Id = id;
        _model.ChildId = id;
        _model.Ppon = ppon;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: null,
            id: id,
            identifier: new Identifier("12345", "Test Org", "DUNS", new Uri("http://test")),
            name: "Test Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, ppon))
            .ReturnsAsync(organisation);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.WarningTagMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Tag_ChildSameAsOrganisation);
        _model.WarningMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ConfirmPage_Warning_ChildSameAsOrganisation);
    }
}