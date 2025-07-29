using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrganisationType = CO.CDP.Tenant.WebApiClient.OrganisationType;
using PartyRole = CO.CDP.Tenant.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class BuyerMouAuthorizationHandlerTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<ILogger<BuyerMouAuthorizationHandler>> _loggerMock = new();
    private readonly Mock<ISession> _sessionMock = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
    private readonly Mock<IUserInfoService> _userInfoServiceMock = new();
    private readonly Guid _organisationId = Guid.NewGuid();

    public BuyerMouAuthorizationHandlerTests()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(UserDetailsFactory.CreateUserDetails(personId: Guid.NewGuid()));
    }

    private BuyerMouAuthorizationHandler CreateHandler()
    {
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserInfoService))).Returns(_userInfoServiceMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
        return new BuyerMouAuthorizationHandler(_sessionMock.Object, _serviceScopeFactoryMock.Object, _organisationClientMock.Object, _loggerMock.Object);
    }

    private static Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext CreateContext() =>
        new([new BuyerMouRequirement()], new System.Security.Claims.ClaimsPrincipal(), null);

    [Fact]
    public async Task HandleRequirementAsync_NoUserDetailsInSession_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns((Models.UserDetails?)null);
        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserDetailsWithoutPersonId_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(UserDetailsFactory.CreateUserDetails(personId: null));
        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

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
    public async Task HandleRequirementAsync_UserWithoutBuyerRole_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(_organisationId);
        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = _organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Supplier }, // No Buyer role
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotFound_DoesNotSucceed()
    {
        SetupUserWithBuyerRole();
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ThrowsAsync(new ApiException("Not found", 404, "", null, null));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIsPendingBuyer_DoesNotSucceed()
    {
        SetupUserWithBuyerRole();
        // Create organisation with pending buyer role (not a valid buyer)
        var organisation = OrganisationFactory.CreateOrganisation(
            id: _organisationId,
            name: "Test Organisation",
            roles: [], // No approved roles
            details: new Details(
                null,
                null,
                [CO.CDP.Organisation.WebApiClient.PartyRole.Buyer], // Buyer role is pending
                null,
                null,
                null,
                null
            )
        );

        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_BuyerWithoutSignedMou_DoesNotSucceed()
    {
        SetupUserWithBuyerRole();
        var organisation = CreateValidBuyerOrganisation();

        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        // MoU signature not found (404) or not latest
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(_organisationId))
            .ThrowsAsync(new ApiException("Not found", 404, "", null, null));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_BuyerWithNonLatestMou_DoesNotSucceed()
    {
        SetupUserWithBuyerRole();
        var organisation = CreateValidBuyerOrganisation();

        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        // MoU signature exists but not latest
        var mouSignature = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User", ["viewer"]),
            _organisationId,
            false,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(_organisationId))
            .ReturnsAsync(mouSignature);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_BuyerWithLatestSignedMou_Succeeds()
    {
        SetupUserWithBuyerRole();
        var organisation = CreateValidBuyerOrganisation();

        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        // MoU signature exists and is latest
        var mouSignature = new MouSignatureLatest(
            new Organisation.WebApiClient.Person("test@example.com", "Test", Guid.NewGuid(), "User", ["viewer"]),
            _organisationId,
            true,
            "TestRole",
            new Mou(DateTimeOffset.UtcNow, "TestFilePath", Guid.NewGuid()),
            "Test Name",
            DateTimeOffset.UtcNow
        );
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(_organisationId))
            .ReturnsAsync(mouSignature);

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ExceptionDuringMouCheck_DoesNotSucceed()
    {
        SetupUserWithBuyerRole();
        var organisation = CreateValidBuyerOrganisation();

        _organisationClientMock.Setup(c => c.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);

        // Unexpected exception during MoU check
        _organisationClientMock.Setup(c => c.GetOrganisationLatestMouSignatureAsync(_organisationId))
            .ThrowsAsync(new Exception("Unexpected error"));

        var handler = CreateHandler();
        var context = CreateContext();

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private void SetupUserWithBuyerRole()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(_organisationId);
        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = _organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Buyer }, // Has Buyer role
                    Type = OrganisationType.Organisation
                }
            }
        });
    }

    private Organisation.WebApiClient.Organisation CreateValidBuyerOrganisation()
    {
        // Valid buyer = has Buyer role and is not in pending roles
        return OrganisationFactory.CreateOrganisation(
            id: _organisationId,
            name: "Test Organisation",
            roles: [CO.CDP.Organisation.WebApiClient.PartyRole.Buyer],
            details: new Details(
                null, // No approval object needed - approval comes from having buyer role
                null,
                [], // Empty pending roles means the buyer role is approved
                null,
                null,
                null,
                null
            )
        );
    }
}