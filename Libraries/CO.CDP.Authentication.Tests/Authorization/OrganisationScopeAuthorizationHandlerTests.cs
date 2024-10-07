using CO.CDP.Authentication.Authorization;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Tests.Authorization;

public class OrganisationScopeAuthorizationHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository = new();
    private readonly OrganisationScopeAuthorizationHandler _handler;

    public OrganisationScopeAuthorizationHandlerTests()
    {
        _handler = new OrganisationScopeAuthorizationHandler(_mockHttpContextAccessor.Object, _mockOrganisationRepository.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenRequirementOrganisationPersonScopeIsEmpty()
    {
        var context = CreateAuthorizationHandlerContext("user-urn", [], OrganisationIdLocation.Path);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenOrganisationIdLocationIsNone()
    {
        var context = CreateAuthorizationHandlerContext("user-urn", ["Admin"], OrganisationIdLocation.None);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData("/url-with-no-organisation-id", OrganisationIdLocation.Path)]
    [InlineData("?query-with-no-organisation-id=", OrganisationIdLocation.QueryString)]
    public async Task HandleRequirementAsync_ShouldFail_WhenOrganisationIdCannotBeFetched(string url, OrganisationIdLocation location)
    {
        var context = CreateAuthorizationHandlerContext("user-urn", ["Admin"], location);
        MockHttpContext(location, url);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }


    [Theory]
    [InlineData(true, "/organisations/{0}", OrganisationIdLocation.Path, true)]
    [InlineData(true, "?organisation-id={0}", OrganisationIdLocation.QueryString, true)]
    [InlineData(false, "/organisations/{0}", OrganisationIdLocation.Path, false)]
    [InlineData(false, "?organisation-id={0}", OrganisationIdLocation.QueryString, false)]
    public async Task HandleRequirementAsync_MatchExpectedResult(bool organisationPersonScopeExists, string urlFormat, OrganisationIdLocation location, bool expectedResult)
    {
        var userUrn = "user-urn";
        var organisationId = Guid.NewGuid();

        var context = CreateAuthorizationHandlerContext(userUrn, organisationPersonScopeExists ? ["Admin"] : ["Scope1"], location);
        MockHttpContext(location, string.Format(urlFormat, organisationId));

        _mockOrganisationRepository.Setup(x => x.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync(GetOrganisationPerson());

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldPass_WhenOrganisationIdLocationIsBody()
    {
        var userUrn = "user-urn";
        var organisationId = Guid.NewGuid();
        var context = CreateAuthorizationHandlerContext(userUrn, ["Admin"], OrganisationIdLocation.Body);
        MockHttpContext(OrganisationIdLocation.Body, organisationId: organisationId);

        _mockOrganisationRepository.Setup(x => x.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync(GetOrganisationPerson());

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenRequirementPersonScopeIsEmpty()
    {
        var context = CreateAuthorizationHandlerContext("user-urn", []);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldPass_WhenRequirementPersonScopeExists()
    {
        var userUrn = "user-urn";
        var context = CreateAuthorizationHandlerContext(userUrn, ["Scope1"], new Claim(ClaimType.Roles, "Scope1,Scope2"));

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldPass_WhenRequirementPersonScopeDoesNotExists()
    {
        var userUrn = "user-urn";
        var context = CreateAuthorizationHandlerContext(userUrn, ["Scope1"]);
        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private void MockHttpContext(OrganisationIdLocation location, string? url = null, Guid? organisationId = null)
    {
        var mockHttpContext = new DefaultHttpContext();

        switch (location)
        {
            case OrganisationIdLocation.Path:
                mockHttpContext.Request.Path = url;
                break;
            case OrganisationIdLocation.QueryString:
                mockHttpContext.Request.QueryString = new QueryString(url);
                break;
            case OrganisationIdLocation.Body:
                mockHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { organisationId })));
                mockHttpContext.Request.ContentType = "application/json";
                break;
        }

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
    }

    private static OrganisationPerson GetOrganisationPerson()
    {
        return new OrganisationPerson
        {
            Organisation = Mock.Of<Organisation>(),
            Person = Mock.Of<Person>(),
            Scopes = ["Admin"]
        };
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        string userUrn, string[] personScopes, Claim? additionalUserClaim = null)
    {
        return CreateAuthorizationHandlerContext(userUrn,
            new OrganisationScopeAuthorizationRequirement(personScopes: personScopes),
            additionalUserClaim);
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        string userUrn, string[] organisationPersonScopes, OrganisationIdLocation orgIdLoc)
    {
        return CreateAuthorizationHandlerContext(userUrn,
            new OrganisationScopeAuthorizationRequirement(organisationPersonScopes, orgIdLoc));
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        string userUrn, OrganisationScopeAuthorizationRequirement requirement, Claim? additionalUserClaim = null)
    {
        List<Claim> userClaims = [new Claim(ClaimType.Channel, Channel.OneLogin), new Claim(ClaimType.Subject, userUrn)];
        if (additionalUserClaim != null)
        {
            userClaims.Add(additionalUserClaim);
        }

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        return new AuthorizationHandlerContext([requirement], claimsPrincipal, new object());
    }
}