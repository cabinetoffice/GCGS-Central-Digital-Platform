using FluentAssertions;
using Moq;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.OrganisationApp.Pages.Support;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Address = CO.CDP.Organisation.WebApiClient.Address;

namespace CO.CDP.OrganisationApp.Tests.Pages.Support;

public class OrganisationApprovalModelTests
{
    private readonly Guid _personId;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<IFlashMessageService> _mockFlashMessageService;
    private readonly OrganisationApprovalModel _organisationApprovalModel;

    public OrganisationApprovalModelTests()
    {
        _personId = new Guid();
        _mockOrganisationClient = new();
        _mockFlashMessageService = new();
        Mock<ISession> mockSession = new();
        mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails
            {
                PersonId = _personId,
                UserUrn = "Something"
            });
        _organisationApprovalModel = new OrganisationApprovalModel(_mockOrganisationClient.Object, mockSession.Object, _mockFlashMessageService.Object);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ShouldReturnPageResult()
    {
        var expectedOrganisation = GivenOrganisation();

        var expectedPersons = new List<CDP.Organisation.WebApiClient.Person>
        {
            new CDP.Organisation.WebApiClient.Person(
                id: Guid.NewGuid(),
                firstName: "Admin",
                lastName: "User",
                email: "admin.user@example.com",
                scopes: new List<string> { "ADMIN", "RESPONDER" }
            ),
            new CDP.Organisation.WebApiClient.Person(
                id: Guid.NewGuid(),
                firstName: "Regular",
                lastName: "User",
                email: "regular.user@example.com",
                scopes: new List<string> { "EDITOR" }
            )
        };

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedPersons);

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();
        _organisationApprovalModel.OrganisationDetails.Should().BeEquivalentTo(expectedOrganisation);
        _organisationApprovalModel.AdminUser.Should().NotBeNull();
        _organisationApprovalModel.AdminUser.Should().BeEquivalentTo(expectedPersons.First(p => p.Scopes.Contains("ADMIN")));
    }

    [Fact]
    public async Task OnGet_WhenOrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        var organisationId = Guid.NewGuid();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(organisationId))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _organisationApprovalModel.OnGet(organisationId);

        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_WhenBuyerIsApproved_ShouldRedirectToOrganisationsPage()
    {
        var organisationId = Guid.NewGuid();
        _organisationApprovalModel.Approval = true;

        _mockOrganisationClient
            .Setup(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<SupportUpdateOrganisation>()))
            .ReturnsAsync(true);

        var result = await _organisationApprovalModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectToPageResult = result as RedirectToPageResult;
        redirectToPageResult!.PageName.Should().Be("Organisations");
        redirectToPageResult.RouteValues.Should().ContainKey("type").WhoseValue.Should().Be("buyer");

        _mockOrganisationClient.Verify(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.Is<SupportUpdateOrganisation>(r =>
            r.Organisation.Approved == true &&
            r.Organisation.ReviewedById == _personId
        )), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenBuyerIsRejected_ShouldRedirectToOrganisationsPage()
    {
        var organisationId = Guid.NewGuid();
        _organisationApprovalModel.Approval = false;
        _organisationApprovalModel.Comments = "Rejected";

        _mockOrganisationClient
            .Setup(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<SupportUpdateOrganisation>()))
            .ReturnsAsync(true);

        var result = await _organisationApprovalModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectToPageResult = result as RedirectToPageResult;
        redirectToPageResult!.PageName.Should().Be("Organisations");
        redirectToPageResult.RouteValues.Should().ContainKey("type").WhoseValue.Should().Be("buyer");

        _mockOrganisationClient.Verify(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.Is<SupportUpdateOrganisation>(r =>
            r.Organisation.Approved == false &&
            r.Organisation.ReviewedById == _personId &&
            r.Organisation.Comment == "Rejected"
        )), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenSearchOrganisationReturnsMatchingOrganisations_ShouldPopulateMatchingOrganisations()
    {
        var expectedOrganisation = GivenOrganisation();

        var expectedMatchingOrganisations = new List<OrganisationSearchResult>
        {
            new OrganisationSearchResult(
                id: Guid.NewGuid(),
                name: "Matching Organisation 1",
                identifier: new Identifier(
                    scheme: "test-scheme",
                    id: "test-id-1",
                    legalName: "Matching Organisation 1",
                    uri: new Uri("https://example.com/1")
                ),
                roles: new List<PartyRole> { PartyRole.Buyer },
                type: OrganisationType.Organisation
            ),
            new OrganisationSearchResult(
                id: Guid.NewGuid(),
                name: "Matching Organisation 2",
                identifier: new Identifier(
                    scheme: "test-scheme",
                    id: "test-id-2",
                    legalName: "Matching Organisation 2",
                    uri: new Uri("https://example.com/2")
                ),
                roles: new List<PartyRole> { PartyRole.Buyer },
                type: OrganisationType.Organisation
            )
        };

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<CDP.Organisation.WebApiClient.Person>());

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(expectedOrganisation.Name, "buyer", 3))
            .ReturnsAsync(expectedMatchingOrganisations);

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();
        _organisationApprovalModel.MatchingOrganisations.Should().NotBeNull();
        _organisationApprovalModel.MatchingOrganisations.Should().BeEquivalentTo(expectedMatchingOrganisations);
    }

    [Fact]
    public async Task OnGet_WhenSearchOrganisationThrows404_ShouldSetMatchingOrganisationsToEmptyList()
    {
        var expectedOrganisation = GivenOrganisation();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<CDP.Organisation.WebApiClient.Person>());

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(expectedOrganisation.Name, "buyer", 3))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();
        _organisationApprovalModel.MatchingOrganisations.Should().NotBeNull();
        _organisationApprovalModel.MatchingOrganisations.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenApprovedReviewExists_ShouldRedirectAndSetFlashMessage()
    {
        var expectedOrganisation = GivenOrganisation();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<Review>
            {
                new(DateTime.Now, "comment", new ReviewedBy(Guid.NewGuid(), "name"), ReviewStatus.Approved)
            });

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id) as RedirectToPageResult;

        Assert.NotNull(result);
        Assert.Equal("Organisations", result.PageName);
        Assert.Equal("buyer", result.RouteValues?["type"]);

        _mockFlashMessageService.Verify(x => x.SetFlashMessage(
            FlashMessageType.Important,
            "{organisationName} has already been approved",
            null,
            null,
            null,
            It.Is<Dictionary<string, string>>(d => d["organisationName"] == expectedOrganisation.Name)
        ), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenReviewStatusIsPending_ShouldNotRedirect()
    {
        var expectedOrganisation = GivenOrganisation();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<CDP.Organisation.WebApiClient.Person>());

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<Review>
            {
                new(null, null, null, ReviewStatus.Pending)
            });

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();

        _mockFlashMessageService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnGetAsync_WhenReviewStatusIsRejected_ShouldNotRedirect()
    {
        var expectedOrganisation = GivenOrganisation();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<CDP.Organisation.WebApiClient.Person>());

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<Review>
            {
                new(DateTime.Now, "comment", new ReviewedBy(Guid.NewGuid(), "name"), ReviewStatus.Rejected)
            });

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();

        _mockFlashMessageService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnGetAsync_WhenNoReviewFound_ShouldNotRedirect()
    {
        var expectedOrganisation = GivenOrganisation();

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(expectedOrganisation.Id))
            .ReturnsAsync(expectedOrganisation);

        _mockOrganisationClient
            .Setup(x => x.GetOrganisationReviewsAsync(expectedOrganisation.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, "", default, null));

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationPersonsAsync(expectedOrganisation.Id))
            .ReturnsAsync(new List<CDP.Organisation.WebApiClient.Person>());

        var result = await _organisationApprovalModel.OnGet(expectedOrganisation.Id);

        result.Should().BeOfType<PageResult>();

        _mockFlashMessageService.VerifyNoOtherCalls();
    }

    private static CDP.Organisation.WebApiClient.Organisation GivenOrganisation()
    {
        return new CDP.Organisation.WebApiClient.Organisation(
                    additionalIdentifiers: new List<Identifier>(),
                    addresses: new List<Address>(),
                    contactPoint: null,
                    id: Guid.NewGuid(),
                    identifier: null,
                    name: "Test Organisation",
                    type: OrganisationType.Organisation,
                    roles: new List<PartyRole>(),
                    details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
                );
    }
}
