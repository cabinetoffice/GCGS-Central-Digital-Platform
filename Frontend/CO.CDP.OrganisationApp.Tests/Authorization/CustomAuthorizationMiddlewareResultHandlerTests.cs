using CO.CDP.OrganisationApp.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PartyRole = CO.CDP.Tenant.WebApiClient.PartyRole;
using OrganisationType = CO.CDP.Tenant.WebApiClient.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class CustomAuthorizationMiddlewareResultHandlerTests
{
    private readonly Mock<IUserInfoService> _userInfoServiceMock;
    private readonly CustomAuthorizationMiddlewareResultHandler _handler;
    private readonly HttpContext _httpContext;
    private readonly AuthorizationPolicy _policy;

    public CustomAuthorizationMiddlewareResultHandlerTests()
    {
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceScopeMock = new Mock<IServiceScope>();
        _userInfoServiceMock = new Mock<IUserInfoService>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService(typeof(IUserInfoService)))
            .Returns(_userInfoServiceMock.Object);

        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        _handler = new CustomAuthorizationMiddlewareResultHandler(serviceScopeFactoryMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();

        _policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new BuyerMouRequirement())
            .Build();
    }

    [Fact]
    public async Task HandleAsync_WhenBuyerMouRequirementForbidden_WithOriginParameter_PreservesOriginInRedirect()
    {
        var organisationId = Guid.NewGuid();
        var origin = "buyer-view";
        _httpContext.Request.QueryString = new QueryString($"?origin={origin}");

        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Buyer },
                    Type = OrganisationType.Organisation
                }
            }
        });
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(organisationId);

        var authorizeResult = PolicyAuthorizationResult.Forbid();

        await _handler.HandleAsync(
            _ => Task.CompletedTask,
            _httpContext,
            _policy,
            authorizeResult);

        _httpContext.Response.StatusCode.Should().Be(302);
        _httpContext.Response.Headers.Location.ToString()
            .Should().Be($"/organisation/{organisationId}/not-signed-memorandum?origin=buyer-view");
    }

    [Fact]
    public async Task HandleAsync_WhenBuyerMouRequirementForbidden_WithoutOriginParameter_RedirectsWithoutOrigin()
    {
        var organisationId = Guid.NewGuid();

        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Buyer },
                    Type = OrganisationType.Organisation
                }
            }
        });
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(organisationId);

        var authorizeResult = PolicyAuthorizationResult.Forbid();

        await _handler.HandleAsync(
            _ => Task.CompletedTask,
            _httpContext,
            _policy,
            authorizeResult);

        _httpContext.Response.StatusCode.Should().Be(302);
        _httpContext.Response.Headers.Location.ToString()
            .Should().Be($"/organisation/{organisationId}/not-signed-memorandum");
    }

    [Fact]
    public async Task HandleAsync_WhenBuyerMouRequirementForbidden_WithOverviewOrigin_PreservesOverviewOrigin()
    {
        var organisationId = Guid.NewGuid();
        var origin = "overview";
        _httpContext.Request.QueryString = new QueryString($"?origin={origin}");

        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Buyer },
                    Type = OrganisationType.Organisation
                }
            }
        });
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(organisationId);

        var authorizeResult = PolicyAuthorizationResult.Forbid();

        await _handler.HandleAsync(
            _ => Task.CompletedTask,
            _httpContext,
            _policy,
            authorizeResult);

        _httpContext.Response.StatusCode.Should().Be(302);
        _httpContext.Response.Headers.Location.ToString()
            .Should().Be($"/organisation/{organisationId}/not-signed-memorandum?origin=overview");
    }

    [Fact]
    public async Task HandleAsync_WhenBuyerMouRequirementForbidden_NoOrganisationId_RedirectsToPageNotFound()
    {
        var organisationId = Guid.NewGuid();

        _userInfoServiceMock.Setup(u => u.GetUserInfo()).ReturnsAsync(new UserInfo
        {
            Name = "Test User",
            Email = "test@test.com",
            Organisations = new List<UserOrganisationInfo>
            {
                new()
                {
                    Id = organisationId,
                    Name = "Test Organisation",
                    Roles = new List<PartyRole> { PartyRole.Buyer },
                    Type = OrganisationType.Organisation
                }
            }
        });
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns((Guid?)null);

        var authorizeResult = PolicyAuthorizationResult.Forbid();

        await _handler.HandleAsync(
            _ => Task.CompletedTask,
            _httpContext,
            _policy,
            authorizeResult);

        _httpContext.Response.StatusCode.Should().Be(302);
        _httpContext.Response.Headers.Location.ToString()
            .Should().Be("/page-not-found");
    }

    [Fact]
    public async Task HandleAsync_WhenNotBuyerMouRequirement_RedirectsToPageNotFound()
    {
        var policyWithoutBuyerMou = new AuthorizationPolicyBuilder()
            .AddRequirements(new DenyAnonymousAuthorizationRequirement())
            .Build();

        var authorizeResult = PolicyAuthorizationResult.Forbid();

        await _handler.HandleAsync(
            _ => Task.CompletedTask,
            _httpContext,
            policyWithoutBuyerMou,
            authorizeResult);

        _httpContext.Response.StatusCode.Should().Be(302);
        _httpContext.Response.Headers.Location.ToString()
            .Should().Be("/page-not-found");
    }
}