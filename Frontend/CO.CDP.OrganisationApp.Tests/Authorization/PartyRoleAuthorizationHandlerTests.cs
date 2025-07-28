using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Tests.TestData;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class PartyRoleAuthorizationHandlerTests
{
    private readonly Mock<ISession> _sessionMock = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
    private readonly Mock<IUserInfoService> _userInfoServiceMock = new();
    private readonly Guid _organisationId = Guid.NewGuid();

    public PartyRoleAuthorizationHandlerTests()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(UserDetailsFactory.CreateUserDetails(personId: Guid.NewGuid()));
    }

    private PartyRoleAuthorizationHandler CreateHandler()
    {
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserInfoService))).Returns(_userInfoServiceMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
        return new PartyRoleAuthorizationHandler(_sessionMock.Object, _serviceScopeFactoryMock.Object);
    }

    private static AuthorizationHandlerContext CreateContext(PartyRole requiredRole) =>
        new([new PartyRoleAuthorizationRequirement(requiredRole)], new System.Security.Claims.ClaimsPrincipal(), null);

    [Fact]
    public async Task HandleRequirementAsync_NoUserDetailsInSession_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns((Models.UserDetails?)null);
        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserDetailsWithoutPersonId_DoesNotSucceed()
    {
        _sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(UserDetailsFactory.CreateUserDetails(personId: null));
        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationIdMissing_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns((Guid?)null);
        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationNotFound_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(_organisationId);
        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>()
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithoutRequiredRole_DoesNotSucceed()
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
                    Roles = new List<PartyRole> { PartyRole.Supplier }, // Has Supplier but not Buyer
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer); // Requesting Buyer role

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithRequiredRole_Succeeds()
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
                    Roles = new List<PartyRole> { PartyRole.Buyer }, // Has required Buyer role
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithMultipleRoles_SucceedsWhenContainsRequired()
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
                    Roles = new List<PartyRole> { PartyRole.Supplier, PartyRole.Buyer, PartyRole.Tenderer },
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ExceptionThrown_DoesNotSucceed()
    {
        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ThrowsAsync(new Exception("Test exception"));
        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithPendingBuyerRole_DoesNotSucceed()
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
                    Roles = new List<PartyRole>(), // No approved roles
                    PendingRoles = new List<PartyRole> { PartyRole.Buyer }, // Buyer role is pending
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithBothApprovedAndPendingRoles_SucceedsForApprovedRole()
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
                    Roles = new List<PartyRole> { PartyRole.Supplier }, // Approved supplier role
                    PendingRoles = new List<PartyRole> { PartyRole.Buyer }, // Buyer role is pending
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Supplier); // Test access to supplier pages

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationWithBothApprovedAndPendingRoles_DoesNotSucceedForPendingRole()
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
                    Roles = new List<PartyRole> { PartyRole.Supplier }, // Approved supplier role
                    PendingRoles = new List<PartyRole> { PartyRole.Buyer }, // Buyer role is pending
                    Type = OrganisationType.Organisation
                }
            }
        });

        var handler = CreateHandler();
        var context = CreateContext(PartyRole.Buyer); // Test access to buyer pages - should fail

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}