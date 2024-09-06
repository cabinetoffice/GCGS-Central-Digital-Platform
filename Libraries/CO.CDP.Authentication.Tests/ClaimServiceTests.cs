using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace CO.CDP.Authentication.Tests;

public class ClaimServiceTests
{
    private readonly Mock<ITenantRepository> mockTenantRepo;

    public ClaimServiceTests()
    {
        mockTenantRepo = new();
    }

    [Fact]
    public void GetUserUrn_ShouldReturnUrn_WhenUserHasSubClaim()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new("sub", userUrn)]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);

        var result = claimService.GetUserUrn();
        result.Should().Be(userUrn);
    }

    [Fact]
    public void GetUserUrn_ShouldReturnNull_WhenUserHasNoSubClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);
        var result = claimService.GetUserUrn();

        result.Should().BeNull();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnFalse_WhenUserHasNoSubClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnFalse_WhenUserHasNoTenant()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new("sub", userUrn)]);

        mockTenantRepo.Setup(m => m.LookupTenant(userUrn))
            .ReturnsAsync((TenantLookup?)default);

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnFalse_WhenDoesNotHaveAccessToOrganisation()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new("sub", userUrn)]);

        mockTenantRepo.Setup(m => m.LookupTenant(userUrn))
            .ReturnsAsync(new TenantLookup
            {
                User = new TenantLookup.PersonUser { Name = "Test", Email = "test@test", Urn = userUrn },
                Tenants = []
            });

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(new Guid("57dcf48c-8910-4108-9cf1-c2935488a085"));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnTrue_WhenDoesHaveAccessToOrganisation()
    {
        var organisationId = new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2");
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new("sub", userUrn)]);

        mockTenantRepo.Setup(m => m.LookupTenant(userUrn))
            .ReturnsAsync(new TenantLookup
            {
                User = new TenantLookup.PersonUser { Name = "Test", Email = "test@test", Urn = userUrn },
                Tenants = [new TenantLookup.Tenant { Id = Guid.NewGuid(), Name = "Ten",
                    Organisations = [new TenantLookup.Organisation { Id = organisationId, Name = "org", Roles = [], Scopes = [] }] }]
            });

        var claimService = new ClaimService(httpContextAccessor.Object, mockTenantRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(organisationId);

        result.Should().BeTrue();
    }

    private static Mock<IHttpContextAccessor> GivenHttpContextWith(List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return httpContextAccessor;
    }
}