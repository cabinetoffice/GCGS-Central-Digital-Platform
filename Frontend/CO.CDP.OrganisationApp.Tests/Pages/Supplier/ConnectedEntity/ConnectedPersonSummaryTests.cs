using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public class ConnectedPersonSummaryTests
{
    private readonly Mock<WebApiClient.IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _sessionMock;
    private readonly ConnectedPersonSummaryModel _model;

    private static readonly System.Guid EntityId = new Guid();

    public ConnectedPersonSummaryTests()
    {
        _mockOrganisationClient = new Mock<WebApiClient.IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedPersonSummaryModel(_mockOrganisationClient.Object, _sessionMock.Object)
        {
            Id = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WithConnectedEntities()
    {
        SetupOrganisationClientMock();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(new ConnectedEntityState());

        var result = await _model.OnGet(true);

        _sessionMock.Verify(s => s.Remove(Session.ConnectedPersonKey), Times.Once);

        result.Should().BeOfType<PageResult>();
        _model.HasConnectedEntity.Should().BeTrue();
        _model.ConnectedEntities.ToList().Should().StartWith(ConnectedEntities.ToList()[0]);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult_WithConnectedEntitiesShowingEndDate()
    {
        var endDate = DateTimeOffset.Now;

        _mockOrganisationClient.Setup(x => x.GetConnectedEntitiesAsync(_model.Id)).ReturnsAsync([
            new ConnectedEntityLookup(
                endDate: endDate,
                entityId: EntityId,
                entityType: ConnectedEntityType.Organisation,
                name: "Rocky Balboa",
                uri: new Uri("http://test")
            )
        ]);

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(new ConnectedEntityState());

        var result = await _model.OnGet(true);

        _sessionMock.Verify(s => s.Remove(Session.ConnectedPersonKey), Times.Once);

        result.Should().BeOfType<PageResult>();
        _model.HasConnectedEntity.Should().BeTrue();
        _model.ConnectedEntities.First().EndDate.Should().Be(endDate);
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_WhenSupplierInfoNotFound()
    {
        _mockOrganisationClient.Setup(x => x.GetConnectedEntitiesAsync(_model.Id))
            .ThrowsAsync(new WebApiClient.ApiException("", 404, "", default, null));

        var result = await _model.OnGet(true);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ReturnsPageResult_WhenModelStateIsInvalid()
    {
        SetupOrganisationClientMock();
        _model.ModelState.AddModelError("HasConnectedEntity", "Please select an option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _mockOrganisationClient.Verify(x => x.GetConnectedEntitiesAsync(_model.Id), Times.Once);
    }

    private static ICollection<ConnectedEntityLookup> ConnectedEntities = [
        new ConnectedEntityLookup(endDate: null, entityId: EntityId, entityType: ConnectedEntityType.Organisation, name: "Rocky Balboa", uri: new Uri("http://test"))
    ];

    private void SetupOrganisationClientMock()
    {
        _mockOrganisationClient.Setup(x => x.GetConnectedEntitiesAsync(_model.Id)).ReturnsAsync(ConnectedEntities);
    }
}