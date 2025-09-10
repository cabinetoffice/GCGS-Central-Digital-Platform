using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationSuccessPageTests
{
    private readonly Mock<ILogger<ChildOrganisationSuccessPage>> _mockLogger;
    private readonly ChildOrganisationSuccessPage _model;
    private readonly Guid _testParentId = Guid.NewGuid();
    private readonly Guid _testChildId = Guid.NewGuid();
    private const string TestOrganisationName = "Test Ppon Organisation";

    public ChildOrganisationSuccessPageTests()
    {
        var mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockLogger = new Mock<ILogger<ChildOrganisationSuccessPage>>();
        var mockAuthorizationService = new Mock<IAuthorizationService>();
        mockAuthorizationService.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var testOrganisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: _testChildId,
            identifier: new Identifier("AAAAA-6666-BBBB", "asd", "PPON", new Uri("http://whatever")),
            name: "Test Ppon Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier, PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(testOrganisation);

        _model = new ChildOrganisationSuccessPage(_mockLogger.Object)
        {
            Id = _testParentId,
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        _model.TempData["ChildName"] = TestOrganisationName;
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var model = new ChildOrganisationSuccessPage(_mockLogger.Object)
        {
            Id = _testParentId
        };

        model.Should().NotBeNull();
        model.Id.Should().Be(_testParentId);
    }



    [Fact]
    public async Task OnGetAsync_WhenSuccessful_ReturnsPageResult()
    {
        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.ChildName.Should().Be(TestOrganisationName);
    }

    [Fact]
    public void BindProperties_ShouldBeSetCorrectly()
    {
        var expectedParentId = Guid.NewGuid();

        _model.Id = expectedParentId;

        _model.Id.Should().Be(expectedParentId);
    }

    [Fact]
    public async Task OnGetAsync_WhenTempDataMissing_RedirectsToError()
    {
        _model.TempData.Clear();

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("/Error");
    }
}
