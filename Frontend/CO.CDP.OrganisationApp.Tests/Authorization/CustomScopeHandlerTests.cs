using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class CustomScopeHandlerTests
{
    private readonly Mock<ISession> _session = new();
    private readonly Mock<HttpContext> _httpContext = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly Mock<IUserInfoService> _userInfoService = new();
    private readonly CustomScopeHandler _handler;
    private readonly UserDetails _defaultUserDetails = new()
    {
        UserUrn = "TestUserUrn",
        PersonId = Guid.NewGuid()
    };

    public CustomScopeHandlerTests()
    {
        var serviceScopeFactory = GivenServiceScopeFactory(new()
        {
            { typeof(IUserInfoService), _userInfoService.Object },
        });
        _session.Setup(x => x.Get<UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext.Object);
        _handler = new CustomScopeHandler(_session.Object, serviceScopeFactory);
    }

    [Theory]
    [InlineData(OrganisationPersonScopes.Viewer, OrganisationPersonScopes.Viewer, null, true)]  // Viewer CAN only do viewer stuff
    [InlineData(OrganisationPersonScopes.Editor, OrganisationPersonScopes.Viewer, null, false)] // Viewer CANNOT do editor stuff
    [InlineData(OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer, null, false)]  // Viewer CANNOT do admin stuff
    [InlineData(OrganisationPersonScopes.Editor, OrganisationPersonScopes.Editor, null, true)]  // Editor CAN do editor stuff
    [InlineData(OrganisationPersonScopes.Viewer, OrganisationPersonScopes.Editor, null, true)]  // Editor CAN do viewer stuff
    [InlineData(OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor, null, false)]  // Editor CANNOT do admin stuff
    [InlineData(OrganisationPersonScopes.Admin, OrganisationPersonScopes.Admin, null, true)]    // Admin CAN do admin stuff
    [InlineData(OrganisationPersonScopes.Editor, OrganisationPersonScopes.Admin, null, true)]   // Admin CAN do editor stuff
    [InlineData(OrganisationPersonScopes.Viewer, OrganisationPersonScopes.Admin, null, true)]   // Admin CAN do viewer stuff
    [InlineData(PersonScopes.SupportAdmin, null, PersonScopes.SupportAdmin, true)]              // SupportAdmin CAN do Support Admin stuff
    [InlineData(PersonScopes.SupportAdmin, OrganisationPersonScopes.Admin, null, false)]        // Admin CANNOT do Support Admin stuff
    [InlineData(PersonScopes.SupportAdmin, OrganisationPersonScopes.Editor, null, false)]       // Editor CANNOT do Support Admin stuff
    [InlineData(PersonScopes.SupportAdmin, OrganisationPersonScopes.Viewer, null, false)]       // Viewer CANNOT do Support Admin stuff
    [InlineData(OrganisationPersonScopes.Viewer, null, PersonScopes.SupportAdmin, true)]        // SupportAdmin CAN do viewer stuff
    [InlineData(OrganisationPersonScopes.Editor, null, PersonScopes.SupportAdmin, false)]       // SupportAdmin CANNOT do editor stuff
    [InlineData(OrganisationPersonScopes.Editor, null, null, false)]                            // No scopes should fail
    [InlineData(PersonScopes.SupportAdmin, null, PersonScopes.SuperAdmin, true)]                // SuperAdmin can do Support Admin stuff
    [InlineData(OrganisationPersonScopes.Admin, null, PersonScopes.SuperAdmin, true)]           // SuperAdmin can do Admin stuff
    [InlineData(OrganisationPersonScopes.Editor, null, PersonScopes.SuperAdmin, true)]          // SuperAdmin can do Editor stuff
    [InlineData(OrganisationPersonScopes.Viewer, null, PersonScopes.SuperAdmin, true)]          // SuperAdmin can do Viewer stuff
    public async Task HandleRequirementAsync_ShouldEvaluateScopesCorrectly(
        string requirementScope,
        string? organisationUserScope,
        string? userScope,
        bool expectedResult)
    {
        var requirement = new ScopeRequirement(requirementScope);
        var context = new AuthorizationHandlerContext([requirement], null!, null);
        var organisationId = Guid.NewGuid();

        _userInfoService.Setup(x => x.GetUserInfo())
            .ReturnsAsync(UserInfo(organisationUserScope, userScope, organisationId));
        _userInfoService.Setup(x => x.GetOrganisationId()).Returns(organisationId);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    private UserInfo UserInfo(string? organisationUserScope, string? userScope, Guid? organisationId = null)
    {
        return new UserInfo
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Scopes = userScope != null ? [userScope] : [],
            Organisations =
            [
                new UserOrganisationInfo
                {
                    Id = organisationId ?? Guid.NewGuid(),
                    Name = "Acme Ltd",
                    Roles = [],
                    PendingRoles = [],
                    Scopes = organisationUserScope != null ? [organisationUserScope] : []
                }
            ]
        };
    }

    private IServiceScopeFactory GivenServiceScopeFactory(Dictionary<Type, object> services)
    {
        Mock<IServiceScopeFactory> serviceScopeFactory = new();
        Mock<IServiceScope> serviceScope = new();

        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
        foreach (var (type, service) in services)
        {
            serviceScope.Setup(x => x.ServiceProvider.GetService(type)).Returns(service);
        }

        return serviceScopeFactory.Object;
    }
}
