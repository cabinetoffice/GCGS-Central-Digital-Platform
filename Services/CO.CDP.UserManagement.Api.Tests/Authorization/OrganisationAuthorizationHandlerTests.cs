using System.Security.Claims;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Interfaces;
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
    public async Task OrganisationAdminHandler_WhenOrganisationExists_Succeeds()
    {
        var requirement = new OrganisationAdminRequirement();
        var orgId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["cdpOrganisationId"] = orgId.ToString();

        var repository = new Mock<IOrganisationRepository>();
        repository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreOrganisation { Id = 1, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org" });

        var handler = new OrganisationAdminHandler(new Mock<ILogger<OrganisationAdminHandler>>().Object, repository.Object);
        var context = BuildContext(requirement, httpContext);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationAdminHandler_WhenOrganisationMissing_DoesNotSucceed()
    {
        var requirement = new OrganisationAdminRequirement();
        var orgId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["cdpOrganisationId"] = orgId.ToString();

        var repository = new Mock<IOrganisationRepository>();
        repository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoreOrganisation?)null);

        var handler = new OrganisationAdminHandler(new Mock<ILogger<OrganisationAdminHandler>>().Object, repository.Object);
        var context = BuildContext(requirement, httpContext);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenOrganisationExists_Succeeds()
    {
        var requirement = new OrganisationMemberRequirement();
        var orgId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["cdpOrganisationId"] = orgId.ToString();

        var repository = new Mock<IOrganisationRepository>();
        repository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreOrganisation { Id = 2, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org" });

        var handler = new OrganisationMemberHandler(new Mock<ILogger<OrganisationMemberHandler>>().Object, repository.Object);
        var context = BuildContext(requirement, httpContext);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationMemberHandler_WhenRouteValueInvalid_DoesNotSucceed()
    {
        var requirement = new OrganisationMemberRequirement();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["cdpOrganisationId"] = "invalid-guid";

        var handler = new OrganisationMemberHandler(new Mock<ILogger<OrganisationMemberHandler>>().Object, new Mock<IOrganisationRepository>().Object);
        var context = BuildContext(requirement, httpContext);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static AuthorizationHandlerContext BuildContext(IAuthorizationRequirement requirement, HttpContext httpContext)
    {
        return new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), httpContext);
    }
}
