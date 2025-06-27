using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationSuccessPageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ILogger<ChildOrganisationSuccessPage>> _mockLogger;
    private readonly ChildOrganisationSuccessPage _model;
    private readonly Guid _testParentId = Guid.NewGuid();
    private readonly Guid _testChildId = Guid.NewGuid();
    private const string TestOrganisationName = "Test Ppon Organisation";

    public ChildOrganisationSuccessPageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockLogger = new Mock<ILogger<ChildOrganisationSuccessPage>>();

        var testOrganisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: _testChildId,
            identifier: new Identifier("AAAAA-6666-BBBB", "asd", "PPON", new Uri("http://whatever")),
            name: "Test Ppon Organisation",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: [CDP.Organisation.WebApiClient.PartyRole.Supplier, CDP.Organisation.WebApiClient.PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(testOrganisation);

        _model = new ChildOrganisationSuccessPage(
            _mockOrganisationClient.Object,
            _mockLogger.Object)
        {
            Id = _testParentId,
            ChildId = _testChildId
        };
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var model = new ChildOrganisationSuccessPage(
            _mockOrganisationClient.Object,
            _mockLogger.Object)
        {
            Id = _testParentId,
            ChildId = _testChildId
        };

        model.Should().NotBeNull();
        model.Id.Should().Be(_testParentId);
        model.ChildId.Should().Be(_testChildId);
    }

    [Fact]
    public async Task OnGetAsync_WhenChildIdIsEmpty_RedirectsToError()
    {
        _model.ChildId = Guid.Empty;

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenChildOrganisationNotFound_RedirectsToError()
    {
        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation)null);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenApiThrowsException_RedirectsToError()
    {
        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WhenSuccessful_ReturnsPageResult()
    {
        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.OrganisationName.Should().Be(TestOrganisationName);
        _model.ChildOrganisation.Should().NotBeNull();
        _model.ChildOrganisation.Name.Should().Be(TestOrganisationName);
    }

    [Fact]
    public void BindProperties_ShouldBeSetCorrectly()
    {
        var expectedParentId = Guid.NewGuid();
        var expectedChildId = Guid.NewGuid();
        const string expectedOrgName = "Updated Organisation";

        _model.Id = expectedParentId;
        _model.ChildId = expectedChildId;
        _model.OrganisationName = expectedOrgName;

        _model.Id.Should().Be(expectedParentId);
        _model.ChildId.Should().Be(expectedChildId);
        _model.OrganisationName.Should().Be(expectedOrgName);
    }
}
