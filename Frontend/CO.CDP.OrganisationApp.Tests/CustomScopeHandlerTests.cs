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

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IUserInfoService)))
            .Returns(_userInfoServiceMock.Object);

        _handler = new CustomScopeHandler(_sessionMock.Object, _serviceScopeFactoryMock.Object);

        _defaultPersonId = new Guid();

        _defaultUserDetails = new Models.UserDetails
        {
            UserUrn = "TestUserUrn",
            PersonId = _defaultPersonId
        };
    }

    [Theory]
    [InlineData(OrganisationPersonScopes.Admin, OrganisationPersonScopes.Admin, null, true)] // Admin should succeed
    [InlineData(PersonScopes.SupportAdmin, OrganisationPersonScopes.Admin, null, false)] // Admin cannot do support admin tasks
    [InlineData(PersonScopes.SupportAdmin, null, PersonScopes.SupportAdmin, true)] // SupportAdmin should succeed
    [InlineData(OrganisationPersonScopes.Editor, null, null, false)] // No scopes should fail
    [InlineData(OrganisationPersonScopes.Viewer, OrganisationPersonScopes.Editor, null, true)] // Editor implies viewer permission
    [InlineData(OrganisationPersonScopes.Viewer, null, PersonScopes.SupportAdmin, true)] // SupportAdmin implies viewer permission
    public async Task HandleRequirementAsync_ShouldEvaluateScopesCorrectly(
        string requirementScope,
        string? organisationUserScope,
        string? userScope,
        bool expectedResult)
    {
        var requirement = new ScopeRequirement(requirementScope);
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey))
                    .Returns(_defaultUserDetails);

        if (organisationUserScope != null)
        {
            _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
                .ReturnsAsync(new[] { organisationUserScope });
        }
        else
        {
            _userInfoServiceMock.Setup(x => x.GetOrganisationUserScopes())
                .ReturnsAsync(new string[] { });
        }

        if (userScope != null)
        {
            _userInfoServiceMock.Setup(x => x.GetUserScopes())
                .ReturnsAsync(new[] { userScope });
        }
        else
        {
            _userInfoServiceMock.Setup(x => x.GetUserScopes())
                .ReturnsAsync(new string[] { });
        }

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }
}
