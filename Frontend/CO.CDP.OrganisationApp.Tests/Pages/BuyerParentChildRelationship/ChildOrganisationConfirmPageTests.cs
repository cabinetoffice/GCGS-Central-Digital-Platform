using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationConfirmPageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ILogger<ChildOrganisationConfirmPage>> _mockLogger;
    private readonly ChildOrganisationConfirmPage _model;

    public ChildOrganisationConfirmPageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockLogger = new Mock<ILogger<ChildOrganisationConfirmPage>>();
        _model = new ChildOrganisationConfirmPage(_mockOrganisationClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task OnGetAsync_WhenLookupThrowsException_RedirectsToErrorPage()
    {
        var id = Guid.NewGuid();
        const string query = "test query";
        const string selectedPponIdentifier = "GB-PPON:12345";

        _model.Id = id;
        _model.Ppon = selectedPponIdentifier;

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, selectedPponIdentifier))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _model.OnGetAsync();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WithValidPponIdentifier_SetsOrganisation()
    {
        var id = Guid.NewGuid();
        const string query = "test query";
        const string selectedPponIdentifier = "GB-PPON:12345";
        var organisationId = Guid.NewGuid();

        _model.Id = id;
        _model.Ppon = selectedPponIdentifier;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: organisationId,
            identifier: new Identifier(selectedPponIdentifier, "Test Org", "PPON", new Uri("http://whatever")),
            name: "Test Organisation",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: [CDP.Organisation.WebApiClient.PartyRole.Supplier, CDP.Organisation.WebApiClient.PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, selectedPponIdentifier))
            .ReturnsAsync(organisation);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.ChildOrganisation.Should().NotBeNull();
        _model.ChildOrganisation.Name.Should().Be("Test Organisation");
        _model.ChildOrganisation.OrganisationId.Should().Be(organisationId);
    }
}
