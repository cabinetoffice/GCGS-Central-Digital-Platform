using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class CustomScopeHandlerTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IUserInfoService> _userInfoServiceMock;
    private readonly CustomScopeHandler _handler;
    private readonly Guid _defaultPersonId;
    private readonly Models.UserDetails _defaultUserDetails;

    public CustomScopeHandlerTests()
    {
        _sessionMock = new Mock<ISession>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _userInfoServiceMock = new Mock<IUserInfoService>();

        // Set up Service Scope Factory to return the mock scope
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IUserInfoService)))
            .Returns(_userInfoServiceMock.Object);

        // Instantiate the handler with the mocked dependencies
        _handler = new CustomScopeHandler(_sessionMock.Object, _serviceScopeFactoryMock.Object);

        _defaultPersonId = new Guid();

        _defaultUserDetails = new Models.UserDetails
        {
            UserUrn = "TestUserUrn",
            PersonId = _defaultPersonId
        };
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenAdminScope_AndNotSupportAdmin()
    {
        var requirement = new ScopeRequirement(OrganisationPersonScopes.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync(new[] { OrganisationPersonScopes.Admin });

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenAdminScope_AndSupportAdminRequired()
    {
        var requirement = new ScopeRequirement(PersonScopes.SupportAdmin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync(new[] { OrganisationPersonScopes.Admin });

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenSupportAdminScope_IsPresent()
    {
        var requirement = new ScopeRequirement(PersonScopes.SupportAdmin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync([]);
        _userInfoServiceMock.Setup(x => x.GetUserScopes())
            .ReturnsAsync(new[] { PersonScopes.SupportAdmin });

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenNoScopesMatch()
    {
        var requirement = new ScopeRequirement(OrganisationPersonScopes.Editor);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync([]);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync([]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenViewerScope_AndEditorOrSupportAdminPresent()
    {
        var requirement = new ScopeRequirement(OrganisationPersonScopes.Viewer);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey)).Returns(_defaultUserDetails);
        _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
            .ReturnsAsync(new[] { OrganisationPersonScopes.Editor });
        _userInfoServiceMock.Setup(x => x.GetUserScopes())
            .ReturnsAsync(new string[] { PersonScopes.SupportAdmin });

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
}
