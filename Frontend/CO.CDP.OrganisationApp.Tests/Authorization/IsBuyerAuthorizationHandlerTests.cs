using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class IsBuyerAuthorizationHandlerTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<IsBuyerAuthorizationHandler>> _loggerMock = new();

    private IsBuyerAuthorizationHandler CreateHandler() =>
        new(_organisationClientMock.Object, _loggerMock.Object);

    private static AuthorizationHandlerContext CreateContext(Guid organisationId) =>
        new([new IsBuyerRequirement()], new System.Security.Claims.ClaimsPrincipal(), organisationId);

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIdMissing_DoesNotSucceed()
    {
        var handler = CreateHandler();
        var context = CreateContext(Guid.Empty);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotFound_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId))
            .ReturnsAsync((Organisation.WebApiClient.Organisation)null!);
        var handler = CreateHandler();
        var context = CreateContext(orgId);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotBuyerOrPending_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var organisation = OrganisationFactory.CreateOrganisation(
            id: orgId,
            roles: new List<PartyRole>(),
            details: new Details(approval: null, buyerInformation: null,
                pendingRoles: new List<PartyRole> { PartyRole.Buyer },
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var handler = CreateHandler();
        var context = CreateContext(orgId);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MouSignatureNotFound_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId))
            .ThrowsAsync(new ApiException("Not found", 404, "", null, null));
        var handler = CreateHandler();
        var context = CreateContext(orgId);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MouSignatureNotLatest_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var mou = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User",
                new List<string> { OrganisationPersonScopes.Viewer }),
            Guid.NewGuid(),
            false,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ReturnsAsync(mou);
        var handler = CreateHandler();
        var context = CreateContext(orgId);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_AllChecksPass_Succeeds()
    {
        var orgId = Guid.NewGuid();
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var mou = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User",
                new List<string> { OrganisationPersonScopes.Viewer }),
            Guid.NewGuid(),
            true,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ReturnsAsync(mou);
        var handler = CreateHandler();
        var context = CreateContext(orgId);
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeTrue();
    }
}