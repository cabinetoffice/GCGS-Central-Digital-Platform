using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class BuyerRoleAuthorizationHandlerTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<BuyerRoleAuthorizationHandler>> _loggerMock = new();
    private readonly Mock<ISession> _sessionMock = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
    private readonly Mock<IUserInfoService> _userInfoServiceMock = new();

    public BuyerRoleAuthorizationHandlerTests()
    {
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { PersonId = Guid.NewGuid(), UserUrn = "urn:test" });
    }

    private BuyerRoleAuthorizationHandler CreateHandler()
    {
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserInfoService))).Returns(_userInfoServiceMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
        return new BuyerRoleAuthorizationHandler(_sessionMock.Object, _serviceScopeFactoryMock.Object, _organisationClientMock.Object, _loggerMock.Object);
    }

    private static AuthorizationHandlerContext CreateContext() =>
        new([new IsBuyerRequirement()], new System.Security.Claims.ClaimsPrincipal(), null);

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIdMissing_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns((Guid?)null);
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotFound_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId))
            .ReturnsAsync((Organisation.WebApiClient.Organisation)null!);
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotBuyerOrPending_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation = OrganisationFactory.CreateOrganisation(
            id: orgId,
            roles: [],
            details: new Details(approval: null, buyerInformation: null,
                pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MouSignatureNotFound_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId))
            .ThrowsAsync(new ApiException("Not found", 404, "", null, null));
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MouSignatureNotLatest_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var mou = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User",
                ["viewer"]),
            Guid.NewGuid(),
            false,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ReturnsAsync(mou);
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_AllChecksPass_Succeeds()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation =
            OrganisationFactory.CreateOrganisation(id: orgId, roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var mou = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User",
                ["viewer"]),
            Guid.NewGuid(),
            true,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ReturnsAsync(mou);
        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_GetOrganisationThrowsException_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ThrowsAsync(new Exception("API error"));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_GetLatestMouSignatureThrowsException_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation = OrganisationFactory.CreateOrganisation(id: orgId, roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ThrowsAsync(new Exception("API error"));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserInfoServiceThrowsException_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ThrowsAsync(new Exception("Service error"));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserDetailsMissing_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey)).Returns((UserDetails)null!);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIdIsEmptyGuid_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(Guid.Empty);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIsPendingBuyer_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation = OrganisationFactory.CreateOrganisation(
            id: orgId,
            roles: [PartyRole.Buyer],
            details: new Details(null, null, [PartyRole.Buyer], null, null, null, null)
        );
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserDetailsWithNullPersonId_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey)).Returns(new UserDetails { UserUrn = "urn:test", PersonId = null });

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIsSupplierAndPendingBuyer_DoesNotSucceed()
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation = OrganisationFactory.CreateOrganisation(
            id: orgId,
            roles: [PartyRole.Supplier],
            details: new Details(null, null, [PartyRole.Buyer], null, null, null, null)
        );
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    public static IEnumerable<object[]> NonBuyerRoles()
    {
        yield return [PartyRole.ProcuringEntity];
        yield return [PartyRole.Supplier];
        yield return [PartyRole.Tenderer];
        yield return [PartyRole.Funder];
        yield return [PartyRole.Enquirer];
        yield return [PartyRole.Payer];
        yield return [PartyRole.Payee];
        yield return [PartyRole.ReviewBody];
        yield return [PartyRole.InterestedParty];
    }

    [Theory]
    [MemberData(nameof(NonBuyerRoles))]
    public async Task HandleRequirementAsync_NonBuyerRoles_DoesNotSucceed(PartyRole nonBuyerRole)
    {
        var orgId = Guid.NewGuid();
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(orgId);
        var organisation = OrganisationFactory.CreateOrganisation(id: orgId, roles: [nonBuyerRole]);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(orgId)).ReturnsAsync(organisation);
        var mou = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User",
                ["viewer"]),
            Guid.NewGuid(),
            true,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(orgId)).ReturnsAsync(mou);

        var handler = CreateHandler();
        var context = CreateContext();
        await handler.HandleAsync(context);
        context.HasSucceeded.Should().BeFalse();
    }
}