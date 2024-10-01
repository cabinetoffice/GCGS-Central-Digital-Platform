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
    private readonly OrganisationApprovalModel _organisationApprovalModel;

    public OrganisationApprovalModelTests()
    {
        _personId = new Guid();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        Mock<ISession> mockSession = new();
        mockSession.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails
            {
                PersonId = _personId,
                UserUrn = "Something"
            });
        _organisationApprovalModel = new OrganisationApprovalModel(_mockOrganisationClient.Object, mockSession.Object);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ShouldReturnPageResult()
    {
        var organisationId = Guid.NewGuid();
        var expectedOrganisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: new List<Identifier>(),
            addresses: new List<Address>(),
            null,
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
            .Setup(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<SupportUpdateOrganisation>()))
            .ReturnsAsync(true);

        var result = await _organisationApprovalModel.OnPost(organisationId);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectToPageResult = result as RedirectToPageResult;
        redirectToPageResult!.PageName.Should().Be("Organisations");
        redirectToPageResult.RouteValues.Should().ContainKey("type").WhoseValue.Should().Be("buyer");

        _mockOrganisationClient.Verify(client => client.SupportUpdateOrganisationAsync(It.IsAny<Guid>(), It.Is<SupportUpdateOrganisation>(r =>
            r.Organisation.Approved == true &&
            r.Organisation.ReviewedById == _personId &&
            r.Organisation.Comment == "Approved"
        )), Times.Once);
    }
}
