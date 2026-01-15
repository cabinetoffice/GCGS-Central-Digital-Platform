using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Moq;
using PartyRole = CO.CDP.Tenant.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationSelectionTest
{
    private readonly Mock<ISession> _session = new();
    private readonly Mock<IUserInfoService> _userInfoService = new();
    private readonly Mock<IOrganisationClient> _organisationClient = new();

    public OrganisationSelectionTest()
    {
        _session.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public async Task OnGet_WhenSessionIsValid_ShouldPopulatePageModel()
    {
        var model = GivenOrganisationSelectionModelModel();

        _session.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });

        _userInfoService.Setup(s => s.GetUserInfo()).ReturnsAsync(UserInfo());

        await model.OnGet();

        model.UserOrganisations.Should().HaveCount(1);
    }

    [Fact]
    public async Task OnGet_WhenOrgHasPendingRoles_ShouldCallGetOrganisationReviewsAsync()
    {
        var model = GivenOrganisationSelectionModelModel();

        _session.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });

        _userInfoService.Setup(s => s.GetUserInfo()).ReturnsAsync(UserInfo(pendingRoles: [PartyRole.Buyer]));

        _organisationClient.Setup(o => o.GetOrganisationReviewsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([GivenReview(ReviewStatus.Pending)]);

        await model.OnGet();

        _organisationClient.Verify(c => c.GetOrganisationReviewsAsync(It.IsAny<Guid>()), Times.Once);

        model.UserOrganisations.Should().HaveCount(1);
        model.UserOrganisations.FirstOrDefault().Review?.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public async Task OnGet_WhenOrgHasPendingRoles_AndIsRejected_CallToGetOrganisationReviewsAsync_ShouldReturnRejectedStatus()
    {
        var model = GivenOrganisationSelectionModelModel();

        _session.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });

        _userInfoService.Setup(s => s.GetUserInfo()).ReturnsAsync(UserInfo(pendingRoles: [PartyRole.Buyer]));

        _organisationClient.Setup(o => o.GetOrganisationReviewsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([GivenReview(ReviewStatus.Rejected)]);

        await model.OnGet();

        _organisationClient.Verify(c => c.GetOrganisationReviewsAsync(It.IsAny<Guid>()), Times.Once);

        model.UserOrganisations.Should().HaveCount(1);
        model.UserOrganisations.FirstOrDefault().Review?.Status.Should().Be(ReviewStatus.Rejected);
    }

    private OrganisationSelectionModel GivenOrganisationSelectionModelModel()
    {
        return new OrganisationSelectionModel(_userInfoService.Object, _organisationClient.Object, _session.Object);
    }

    private static Review GivenReview(ReviewStatus reviewStatus)
    {
        return new Review(null, null, null, reviewStatus);
    }

    private UserInfo UserInfo(ICollection<PartyRole>? pendingRoles = null)
    {
        return new UserInfo
        {
            Name = "Person A",
            Email = "person@example.com",
            Scopes = ["SUPPORTADMIN"],
            Organisations =
            [
                new UserOrganisationInfo
                {
                    Id = Guid.NewGuid(),
                    Name = "Acme Ltd",
                    Roles = [PartyRole.Tenderer],
                    PendingRoles = pendingRoles ?? [],
                    Scopes = ["VIEWER"]
                }
            ]
        };
    }
}