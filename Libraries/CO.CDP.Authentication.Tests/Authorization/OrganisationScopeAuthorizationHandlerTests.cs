using CO.CDP.Authentication.Authorization;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CO.CDP.Authentication.Tests.Authorization;

public class OrganisationScopeAuthorizationHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly OrganisationScopeAuthorizationHandler _handler;

    public OrganisationScopeAuthorizationHandlerTests()
    {
        _mockHttpContextAccessor = new();
        _mockTenantRepository = new();
        _handler = new OrganisationScopeAuthorizationHandler(_mockHttpContextAccessor.Object, _mockTenantRepository.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenRequirementScopeIsEmpty()
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
    public async Task HandleRequirementAsync_MatchExpectedResult(bool organisationScopeExists, string urlFormat, OrganisationIdLocation location, bool expectedResult)
    {
        var userUrn = "user-urn";
        var organisationId = Guid.NewGuid();

        var context = CreateAuthorizationHandlerContext(userUrn, organisationScopeExists ? ["Admin"] : ["Scope1"], location);
        MockHttpContext(location, string.Format(urlFormat, organisationId));

        _mockTenantRepository.Setup(x => x.LookupTenant(userUrn))
            .ReturnsAsync(GetTenantLookup(userUrn, organisationId));

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    [Fact]
    public async Task FetchOrganisationIdAsync_Should_Return_Correct_OrganisationId_From_Body()
    {
        var userUrn = "user-urn";
        var organisationId = Guid.NewGuid();
        var context = CreateAuthorizationHandlerContext(userUrn, ["Admin"], OrganisationIdLocation.Body);
        MockHttpContext(OrganisationIdLocation.Body, organisationId: organisationId);

        _mockTenantRepository.Setup(x => x.LookupTenant(userUrn))
            .ReturnsAsync(GetTenantLookup(userUrn, organisationId));

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
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

    private static TenantLookup GetTenantLookup(string userUrn, Guid organisationId)
    {
        var tenantLookup = new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = "test@test.com",
                Name = "Test person",
                Urn = userUrn
            },
            Tenants = [
                new TenantLookup.Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Tenant",
                    Organisations = [
                    new TenantLookup.Organisation
                    {
                        Id = organisationId,
                        Name = $"Test Org",
                        Roles = [OrganisationInformation.PartyRole.Buyer],
                        Scopes = ["Admin"]
                    }]
                }]
        };

        return tenantLookup;
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(string userUrn, string[] scopes, OrganisationIdLocation orgIdLoc)
    {
        var requirement = new OrganisationScopeAuthorizationRequirement(scopes, orgIdLoc);
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userUrn)]));
        return new AuthorizationHandlerContext([requirement], claimsPrincipal, new object());
    }
}