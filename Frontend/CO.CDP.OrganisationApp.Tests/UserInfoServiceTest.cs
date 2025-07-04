using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OrganisationType = CO.CDP.Tenant.WebApiClient.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests;

public class UserInfoServiceTest
{
    private readonly IHttpContextAccessor _httpContextAccessor = GivenHttpContext;
    private readonly Mock<ITenantClient> _tenantClient = new();
    private UserInfoService UserInfoService => new(_httpContextAccessor, _tenantClient.Object);

    [Fact]
    public async Task ItGetsUserInfoBasedOnTenantLookup()
    {
        var userOrganisation = GivenUserOrganisation(
            name: "Acme Ltd", scopes: ["ADMIN"], roles: [PartyRole.Tenderer], pendingRoles: [PartyRole.Buyer], type: OrganisationType.InformalConsortium);
        var tenantLookup = GivenTenantLookup(
            userDetails: GivenUserDetails(name: "Bob", email: "bob@example.com", scopes: ["SUPPORT"]),
            tenants: [GivenUserTenant(organisations: [userOrganisation])]
        );

        _tenantClient.Setup(c => c.LookupTenantAsync()).ReturnsAsync(tenantLookup);

        var userInfo = await UserInfoService.GetUserInfo();

        userInfo.Should().BeEquivalentTo(new UserInfo
        {
            Name = "Bob",
            Email = "bob@example.com",
            Scopes = ["SUPPORT"],
            Organisations =
            [
                new UserOrganisationInfo
                {
                    Id = userOrganisation.Id,
                    Name = "Acme Ltd",
                    PendingRoles = [PartyRole.Buyer],
                    Roles = [PartyRole.Tenderer],
                    Scopes = ["ADMIN"],
                    Type = OrganisationType.InformalConsortium
                }
            ]
        });
    }

    [Fact]
    public async Task ItOnlyCallsTenantLookupOncePerRequest()
    {
        var tenantLookup = GivenTenantLookup();

        _tenantClient.Setup(c => c.LookupTenantAsync()).ReturnsAsync(tenantLookup);

        var userInfo1 = await UserInfoService.GetUserInfo();
        var userInfo2 = await UserInfoService.GetUserInfo();
        var userInfo3 = await UserInfoService.GetUserInfo();

        _tenantClient.Verify(c => c.LookupTenantAsync(), Times.Once);
        userInfo1.Should().BeOfType<UserInfo>();
        userInfo2.Should().Be(userInfo1);
        userInfo3.Should().Be(userInfo1);
    }

    [Theory]
    [MemberData(nameof(ViewerScopeCases))]
    public async Task ItChecksIfUserHasViewerScope(
        List<string> userScopes,
        List<string> userOrganisationScopes,
        string organisationId,
        string path,
        bool outcome)
    {
        var tenantLookup = GivenTenantLookup(
            userDetails: GivenUserDetails(scopes: userScopes),
            tenants:
            [
                GivenUserTenant(organisations:
                [
                    GivenUserOrganisation(scopes: ["ADMIN"]),
                    GivenUserOrganisation(id: Guid.Parse(organisationId), scopes: userOrganisationScopes)
                ])
            ]
        );
        GivenRequestPath(path);

        _tenantClient.Setup(c => c.LookupTenantAsync()).ReturnsAsync(tenantLookup);

        (await UserInfoService.IsViewer()).Should().Be(outcome);
    }

    public static IEnumerable<object[]> ViewerScopeCases() => new List<object[]>
    {
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Viewer },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            true
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Viewer },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/non-organisation-page",
            false
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Viewer },
            "5c08b265-8cad-466d-a8d3-c7096c51e2b1",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            false
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Admin },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            false
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            true
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            true
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string> { OrganisationPersonScopes.Admin },
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            false
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string>(),
            "49c8b8de-457a-40ee-9c29-bb1aa900941c",
            "/organisation/49c8b8de-457a-40ee-9c29-bb1aa900941c",
            true
        }
    };

    [Fact]
    public void ItReturnsNoOrganisationIdIfHttpContextIsMissing()
    {
        var httpContextAccessor = GivenMissingHttpContext;
        var userInfoService = new UserInfoService(httpContextAccessor, _tenantClient.Object);

        userInfoService.GetOrganisationId().Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTrueIfUserHasAnyOrganisationsAssigned()
    {
        var userOrganisation = GivenUserOrganisation();
        var tenantLookup = GivenTenantLookup(tenants: [GivenUserTenant(organisations: [userOrganisation])]);

        _tenantClient.Setup(c => c.LookupTenantAsync()).ReturnsAsync(tenantLookup);

        (await UserInfoService.HasOrganisations()).Should().BeTrue();
    }

    [Fact]
    public async Task ItReturnsFalseIfUserHasNoOrganisationsAssigned()
    {
        var tenantLookup = GivenTenantLookup(tenants: [GivenUserTenant(organisations: [])]);

        _tenantClient.Setup(c => c.LookupTenantAsync()).ReturnsAsync(tenantLookup);

        (await UserInfoService.HasOrganisations()).Should().BeFalse();
    }

    [Fact]
    public async Task ItReturnsFalseIfUserInfoCannotBeRetrieved()
    {
        _tenantClient.Setup(c => c.LookupTenantAsync()).ThrowsAsync(new Exception("Failure during tenant lookup."));

        (await UserInfoService.HasOrganisations()).Should().BeFalse();
    }

    [Theory]
    [InlineData("/organisation/a86ba407-b1ec-4184-b70c-6133f8bbed88", "a86ba407-b1ec-4184-b70c-6133f8bbed88")]
    [InlineData("/organisation/a86ba407-b1ec-4184-b70c-6133f8bbed88/profile", "a86ba407-b1ec-4184-b70c-6133f8bbed88")]
    [InlineData("/consortium/a86ba407-b1ec-4184-b70c-6133f8bbed88", "a86ba407-b1ec-4184-b70c-6133f8bbed88")]
    [InlineData("/consortium/a86ba407-b1ec-4184-b70c-6133f8bbed88/whatever", "a86ba407-b1ec-4184-b70c-6133f8bbed88")]
    [InlineData("/organisation/not-a-guid", null)]
    [InlineData("/organisations/a86ba407-b1ec-4184-b70c-6133f8bbed88", null)]
    [InlineData(null, null)]
    public void ItFindsOrganisationIdInThePath(string? path, string? organisationId)
    {
        GivenRequestPath(path);

        UserInfoService.GetOrganisationId().Should().Be(organisationId != null ? Guid.Parse(organisationId) : null);
    }

    private static UserDetails GivenUserDetails(string? name = null, string? email = null, List<string>? scopes = null)
    {
        return new UserDetails(
            email: email ?? "alice@example.com",
            name: name ?? "Alice",
            scopes: scopes ?? [],
            urn: $"urn:2024:{Guid.NewGuid()}"
        );
    }

    private static TenantLookup GivenTenantLookup(
        UserDetails? userDetails = null,
        ICollection<UserTenant>? tenants = null)
    {
        return new TenantLookup(
            user: userDetails ?? GivenUserDetails(),
            tenants: tenants ?? [GivenUserTenant()]
        );
    }

    private static UserTenant GivenUserTenant(ICollection<UserOrganisation>? organisations = null)
    {
        return new UserTenant(
            id: Guid.NewGuid(),
            name: "The Tenant",
            organisations: organisations ?? []
        );
    }

    private static UserOrganisation GivenUserOrganisation(
        Guid? id = null,
        string? name = null,
        ICollection<string>? scopes = null,
        ICollection<PartyRole>? roles = null,
        ICollection<PartyRole>? pendingRoles = null,
        OrganisationType type = OrganisationType.Organisation)
    {
        return new UserOrganisation(
            id: id ?? Guid.NewGuid(),
            name: name,
            type: type,
            pendingRoles: pendingRoles,
            roles: roles,
            scopes: scopes,
            uri: new Uri("https://example.com")
        );
    }

    private void GivenRequestPath(string? path)
    {
        if (_httpContextAccessor.HttpContext is not null)
        {
            _httpContextAccessor.HttpContext.Request.Path = new PathString(path);
        }
        else
        {
            throw new Exception("Http context is missing.");
        }
    }

    private static IHttpContextAccessor GivenHttpContext
        => new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

    private static IHttpContextAccessor GivenMissingHttpContext
        => new HttpContextAccessor { HttpContext = null };
}