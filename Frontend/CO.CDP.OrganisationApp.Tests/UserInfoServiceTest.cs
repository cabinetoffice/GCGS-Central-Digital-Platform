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
        string? name = null,
        ICollection<string>? scopes = null,
        ICollection<PartyRole>? roles = null,
        ICollection<PartyRole>? pendingRoles = null)
    {
        return new UserOrganisation(
            id: Guid.NewGuid(),
            name: name,
            pendingRoles: pendingRoles,
            roles: roles,
            scopes: scopes,
            uri: new Uri("https://example.com")
        );
    }

    private static IHttpContextAccessor GivenHttpContext()
    {
        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}