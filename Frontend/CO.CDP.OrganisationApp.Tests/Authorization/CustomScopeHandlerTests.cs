using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class CustomScopeHandlerTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ITenantClient> _tenantClientMock;
    private readonly CustomScopeHandler _handler;
    private readonly Models.UserDetails _defaultUserDetails;

    public CustomScopeHandlerTests()
    {
        _sessionMock = new Mock<ISession>();
        _httpContextMock = new Mock<HttpContext>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _tenantClientMock = new Mock<ITenantClient>();
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        Mock<IServiceScope> serviceScopeMock = new();

        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(ITenantClient)))
            .Returns(_tenantClientMock.Object);

        _handler = new CustomScopeHandler(_sessionMock.Object, _httpContextAccessorMock.Object, serviceScopeFactoryMock.Object);

        _defaultUserDetails = new Models.UserDetails
        {
            UserUrn = "TestUserUrn",
            PersonId = Guid.NewGuid()
        };
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
    public async Task HandleRequirementAsync_ShouldEvaluateScopesCorrectly(
        string requirementScope,
        string? organisationUserScope,
        string? userScope,
        bool expectedResult)
    {
        var requirement = new ScopeRequirement(requirementScope);
        var context = new AuthorizationHandlerContext([requirement], null!, null);
        var organisationId = Guid.NewGuid();

        _sessionMock.Setup(x => x.Get<Models.UserDetails>(Session.UserDetailsKey))
                    .Returns(_defaultUserDetails);

        _httpContextMock.Setup(x => x.Request.Path)
            .Returns(new PathString("/organisation/" + organisationId));

        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(_httpContextMock.Object);

        _tenantClientMock.Setup(x => x.LookupTenantAsync())
                         .ReturnsAsync(TenantLookupBuilder(organisationUserScope, userScope, organisationId));

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    private TenantLookup TenantLookupBuilder(string? organisationUserScope, string? userScope, Guid? organisationId = null)
    {
        var organisations = new List<UserOrganisation>()
        {
            new(organisationId ?? Guid.NewGuid(), string.Empty, [], [], organisationUserScope != null ? [organisationUserScope] : [], null)
        };

        var tenants = new List<UserTenant>
        {
            new(Guid.NewGuid(), string.Empty, organisations)
        };

        var user = new UserDetails(string.Empty, string.Empty, userScope != null ? [userScope] : [], _defaultUserDetails.UserUrn);

        return new TenantLookup(tenants, user);
    }
}
