using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CO.CDP.Authentication.Tests;

public class ClaimServiceTests
{
    [Fact]
    public void GetUserUrn_ShouldReturnUrn_WhenUserHasSubClaim()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new("sub", userUrn)]);

        var claimService = new ClaimService(httpContextAccessor.Object);

        var result = claimService.GetUserUrn();
        result.Should().Be(userUrn);
    }

    [Fact]
    public void GetUserUrn_ShouldReturnNull_WhenUserHasNoSubClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.GetUserUrn();

        result.Should().BeNull();
    }

    [Fact]
    public void HaveAccessToOrganisation_ShouldReturnFalse_WhenUserHasNoTenClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.HaveAccessToOrganisation(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveAccessToOrganisation_ShouldReturnFalse_WhenDoesNotHaveAccessToOrganisation()
    {
        var httpContextAccessor = GivenHttpContextWith([new("ten", BuildTenTokenValue())]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.HaveAccessToOrganisation(new Guid("57dcf48c-8910-4108-9cf1-c2935488a085"));

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveAccessToOrganisation_ShouldReturnTrue_WhenDoesHaveAccessToOrganisation()
    {
        var organisationId = new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2");
        var httpContextAccessor = GivenHttpContextWith([new("ten", BuildTenTokenValue(organisationId))]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.HaveAccessToOrganisation(organisationId);

        result.Should().BeTrue();
    }

    private string BuildTenTokenValue(Guid? organisationToAdd = null)
    {
        var tenantLookup = new OrganisationInformation.TenantLookup
        {
            User = new OrganisationInformation.UserDetails { Email = "t@t", Name = "Dave", Urn = "urn:fdc:gov.uk:2022:43af5a8b" },
            Tenants = [new OrganisationInformation.UserTenant
            {
                Id = Guid.NewGuid(),
                Name = "Ten1",
                Organisations = [new OrganisationInformation.UserOrganisation
                {
                    Id = organisationToAdd ?? Guid.NewGuid(),
                    Name = "org",
                    Roles = [],
                    Scopes = [],
                    Uri = new Uri("http://test.com"),
                }]
            }]
        };

        return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tenantLookup))));
    }

    private static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, false))
        {
            gzipStream.Write(data);
        }

        return compressedStream.ToArray();
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