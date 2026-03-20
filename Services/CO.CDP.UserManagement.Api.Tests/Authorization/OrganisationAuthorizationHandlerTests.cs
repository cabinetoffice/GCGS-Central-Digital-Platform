using System.Security.Claims;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Authorization;

public class OrganisationAuthorizationHandlerTests
{
    [Fact]
    public async Task OrganisationMemberHandler_WhenActiveMembershipExists_Succeeds()
    {
        const string userPrincipalId = "member-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMembership(OrganisationRole.Member, isActive: true));

        var handler = new OrganisationMemberHandler(
            new Mock<ILogger<OrganisationMemberHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationMemberRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(ClaimTypes.NameIdentifier, userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenMembershipIsInactive_DoesNotSucceed()
    {
        const string userPrincipalId = "member-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMembership(OrganisationRole.Member, isActive: false));

        var handler = new OrganisationMemberHandler(
            new Mock<ILogger<OrganisationMemberHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationMemberRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(ClaimTypes.NameIdentifier, userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenMembershipIsMissing_DoesNotSucceed()
    {
        const string userPrincipalId = "member-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var handler = new OrganisationMemberHandler(
            new Mock<ILogger<OrganisationMemberHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationMemberRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(ClaimTypes.NameIdentifier, userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenUserPrincipalMissing_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        var handler = new OrganisationMemberHandler(
            new Mock<ILogger<OrganisationMemberHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationMemberRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(claimType: null, claimValue: null));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        organisationRepository.Verify(
            repo => repo.GetByCdpGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        membershipRepository.Verify(
            repo => repo.GetByUserAndOrganisationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OrganisationAdminHandler_WhenUserIsAdmin_Succeeds()
    {
        const string userPrincipalId = "admin-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMembership(OrganisationRole.Admin, isActive: true));

        var handler = new OrganisationAdminHandler(
            new Mock<ILogger<OrganisationAdminHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationAdminRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal("sub", userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationAdminHandler_WhenUserIsOwner_Succeeds()
    {
        const string userPrincipalId = "owner-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMembership(OrganisationRole.Owner, isActive: true));

        var handler = new OrganisationAdminHandler(
            new Mock<ILogger<OrganisationAdminHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationAdminRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(ClaimTypes.NameIdentifier, userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationAdminHandler_WhenUserIsNotAdmin_DoesNotSucceed()
    {
        const string userPrincipalId = "member-123";
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrganisation(orgId, 1));

        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        membershipRepository.Setup(repo => repo.GetByUserAndOrganisationAsync(userPrincipalId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMembership(OrganisationRole.Member, isActive: true));

        var handler = new OrganisationAdminHandler(
            new Mock<ILogger<OrganisationAdminHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationAdminRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(ClaimTypes.NameIdentifier, userPrincipalId));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task OrganisationAdminHandler_WhenUserPrincipalMissing_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var organisationRepository = new Mock<IOrganisationRepository>();
        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        var handler = new OrganisationAdminHandler(
            new Mock<ILogger<OrganisationAdminHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationAdminRequirement(),
            BuildHttpContext(orgId),
            BuildPrincipal(claimType: null, claimValue: null));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        organisationRepository.Verify(
            repo => repo.GetByCdpGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        membershipRepository.Verify(
            repo => repo.GetByUserAndOrganisationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenRouteValueInvalid_DoesNotSucceed()
    {
        var organisationRepository = new Mock<IOrganisationRepository>();
        var membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        var handler = new OrganisationMemberHandler(
            new Mock<ILogger<OrganisationMemberHandler>>().Object,
            organisationRepository.Object,
            membershipRepository.Object);
        var context = BuildContext(
            new OrganisationMemberRequirement(),
            BuildHttpContext("invalid-guid"),
            BuildPrincipal(ClaimTypes.NameIdentifier, "member-123"));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        organisationRepository.Verify(
            repo => repo.GetByCdpGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        membershipRepository.Verify(
            repo => repo.GetByUserAndOrganisationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AuthorizationHandlerContext BuildContext(
        IAuthorizationRequirement requirement,
        HttpContext httpContext,
        ClaimsPrincipal principal)
    {
        return new AuthorizationHandlerContext(new[] { requirement }, principal, httpContext);
    }

    private static DefaultHttpContext BuildHttpContext(Guid organisationId) =>
        BuildHttpContext(organisationId.ToString());

    private static DefaultHttpContext BuildHttpContext(string organisationId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["cdpOrganisationId"] = organisationId;
        return httpContext;
    }

    private static ClaimsPrincipal BuildPrincipal(string? claimType, string? claimValue)
    {
        var claims = claimType != null && claimValue != null
            ? new[] { new Claim(claimType, claimValue) }
            : [];

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static UserOrganisationMembership CreateMembership(OrganisationRole organisationRole, bool isActive)
    {
        return new UserOrganisationMembership
        {
            UserPrincipalId = "user",
            OrganisationId = 1,
            OrganisationRoleId = (int)organisationRole,
            IsActive = isActive
        };
    }

    private static CoreOrganisation CreateOrganisation(Guid cdpOrganisationId, int organisationId)
    {
        return new CoreOrganisation
        {
            Id = organisationId,
            CdpOrganisationGuid = cdpOrganisationId,
            Name = "Org",
            Slug = "org"
        };
    }
}
