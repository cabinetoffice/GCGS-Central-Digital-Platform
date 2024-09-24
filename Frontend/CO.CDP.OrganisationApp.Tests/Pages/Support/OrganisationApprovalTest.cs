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
    private Guid PersonId;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _mockSession;
    private readonly OrganisationApprovalModel _organisationApprovalModel;

    public OrganisationApprovalModelTests()
    {
        PersonId = new Guid();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();
        _mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails
            {
                PersonId = PersonId,
                UserUrn = "Something"
            });
        _organisationApprovalModel = new OrganisationApprovalModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ShouldReturnPageResult()
    {
        var organisationId = Guid.NewGuid();
        var expectedOrganisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: new List<Identifier>(),
            addresses: new List<Address>(),
            contactPoint: null,
            id: organisationId,
            identifier: null,
            name: "Test Organisation",
            roles: new List<PartyRole>()
        );

        _mockOrganisationClient
            .Setup(client => client.GetOrganisationAsync(organisationId))
            .ReturnsAsync(expectedOrganisation);

        var result = await _organisationApprovalModel.OnGet(organisationId);

        result.Should().BeOfType<PageResult>();
        _organisationApprovalModel.OrganisationDetails.Should().BeEquivalentTo(expectedOrganisation);
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
    public async Task OnPost_WhenReviewIsSuccessful_ShouldRedirectToOrganisationsPage()
    {
        var organisationId = Guid.NewGuid();
        _organisationApprovalModel.Approval = true;
        _organisationApprovalModel.Comments = "Approved";

        _mockOrganisationClient
            .Setup(client => client.ReviewOrganisationAsync(It.IsAny<ReviewOrganisation>()))
            .ReturnsAsync(true);

        var result = await _organisationApprovalModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectToPageResult = result as RedirectToPageResult;
        redirectToPageResult!.PageName.Should().Be("Organisations");
        redirectToPageResult.RouteValues.Should().ContainKey("type").WhoseValue.Should().Be("buyer");

        _mockOrganisationClient.Verify(client => client.ReviewOrganisationAsync(It.Is<ReviewOrganisation>(r =>
            r.Approved == true &&
            r.ApprovedById == PersonId &&
            r.Comment == "Approved" &&
            r.OrganisationId == organisationId
        )), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenApprovalIsNotSet_ShouldDefaultToFalseAndRedirect()
    {
        var organisationId = Guid.NewGuid();
        _organisationApprovalModel.Approval = null;
        _organisationApprovalModel.Comments = null;

        _mockOrganisationClient
            .Setup(client => client.ReviewOrganisationAsync(It.IsAny<ReviewOrganisation>()))
            .ReturnsAsync(true);

        var result = await _organisationApprovalModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectToPageResult = result as RedirectToPageResult;
        redirectToPageResult!.PageName.Should().Be("Organisations");
        redirectToPageResult.RouteValues.Should().ContainKey("type").WhoseValue.Should().Be("buyer");

        _mockOrganisationClient.Verify(client => client.ReviewOrganisationAsync(It.Is<ReviewOrganisation>(r =>
            r.Approved == false &&
            r.ApprovedById == PersonId &&
            r.Comment == "" &&
            r.OrganisationId == organisationId
        )), Times.Once);
    }
}
