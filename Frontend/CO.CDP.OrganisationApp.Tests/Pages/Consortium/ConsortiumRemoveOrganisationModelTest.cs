using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumRemoveOrganisationModelTests
{
    private readonly Mock<IOrganisationClient> _orgClientMock = new();
    private readonly Mock<ITempDataService> _tempDataServiceMock = new();
    private readonly ConsortiumRemoveOrganisationModel _model;

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _partyId = Guid.NewGuid();

    public ConsortiumRemoveOrganisationModelTests()
    {
        _model = new ConsortiumRemoveOrganisationModel(_orgClientMock.Object, _tempDataServiceMock.Object)
        {
            Id = _orgId,
            PartyId = _partyId
        };
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenOrganisationAndPartyExist()
    {
        _orgClientMock.Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(GivenOrganisationClientModel("Test Consortium"));

        _orgClientMock.Setup(c => c.GetOrganisationPartiesAsync(_orgId))
            .ReturnsAsync(new OrganisationParties(
            [
                new(_partyId, "Party A", new OrganisationPartyShareCode( DateTimeOffset.UtcNow, "ShareCode1" ) )
            ]));

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.ConsortiumName.Should().Be("Test Consortium");
        _model.PartyName.Should().Be("Party A");
    }

    [Fact]
    public async Task OnGet_ShouldRedirect_WhenOrganisationNotFound()
    {
        _orgClientMock.Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync((CDP.Organisation.WebApiClient.Organisation?)null);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldRedirect_WhenPartyNotFound()
    {
        _orgClientMock.Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(GivenOrganisationClientModel("Test Consortium"));

        _orgClientMock.Setup(c => c.GetOrganisationPartiesAsync(_orgId))
            .ReturnsAsync(new OrganisationParties([]));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToOverview_WhenConfirmed()
    {
        _orgClientMock.Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(GivenOrganisationClientModel("Test Consortium"));

        _orgClientMock.Setup(c => c.GetOrganisationPartiesAsync(_orgId))
            .ReturnsAsync(new OrganisationParties(
            [
                new(_partyId, "Party A", new OrganisationPartyShareCode( DateTimeOffset.UtcNow, "ShareCode1" ) )
            ]));

        _model.ConfirmRemove = true;


        var result = await _model.OnPost();

        _tempDataServiceMock.Verify(t => t.Put("ConsortiumOrganisationRemoved", "Party A"), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    [Fact]
    public async Task OnPost_ShouldSkipRemoval_WhenConfirmRemoveIsFalse()
    {
        _orgClientMock.Setup(c => c.GetOrganisationAsync(_orgId))
            .ReturnsAsync(GivenOrganisationClientModel("Test Consortium"));

        _orgClientMock.Setup(c => c.GetOrganisationPartiesAsync(_orgId))
            .ReturnsAsync(new OrganisationParties(
            [
                new(_partyId, "Party A", new OrganisationPartyShareCode( DateTimeOffset.UtcNow, "ShareCode1" ) )
            ]));

        _model.ConfirmRemove = false;

        var result = await _model.OnPost();

        _orgClientMock.Verify(c => c.RemoveOrganisationPartyAsync(It.IsAny<Guid>(), It.IsAny<RemoveOrganisationParty>()), Times.Never);
        result.Should().BeOfType<RedirectToPageResult>();
    }

    private CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(string orgName)
    {
        return new CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null,
            contactPoint: null, id: _orgId, identifier: null, name: "Test Consortium",
            type: OrganisationType.InformalConsortium, roles: [],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }
}