using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;

public class UserInfoServiceTest
{
    private readonly IHttpContextAccessor _httpContextAccessor = GivenHttpContext();
    private readonly Mock<ITenantClient> _tenantClient = new();
    private IUserInfoService UserInfoService => new UserInfoService(_httpContextAccessor, _tenantClient.Object);

    [Fact]
    public async Task ItGetsUserInfoBasedOnTenantLookup()
    {
        var userOrganisation = GivenUserOrganisation(
            name: "Acme Ltd", scopes: ["ADMIN"], roles: [PartyRole.Tenderer], pendingRoles: [PartyRole.Buyer]);
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
                    Scopes = ["ADMIN"]
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
        },
    };

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
        ICollection<PartyRole>? pendingRoles = null)
    {
        return new UserOrganisation(
            id: id ?? Guid.NewGuid(),
            name: name,
            pendingRoles: pendingRoles,
            roles: roles,
            scopes: scopes,
            uri: new Uri("https://example.com")
        );
    }

    private void GivenRequestPath(string path)
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

    private static IHttpContextAccessor GivenHttpContext()
    {
        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}