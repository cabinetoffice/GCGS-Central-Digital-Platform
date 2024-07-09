using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace CO.CDP.Authentication.Tests;
public class ClaimServiceTests
{
    [Fact]
    public void GetUserUrn_ShouldReturnUrn_WhenUserHasSubClaim()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var claims = new List<Claim> { new Claim("sub", userUrn) };
        var httpContextAccessor = GivenHttpContextWith(claims);

        var claimService = new ClaimService(httpContextAccessor.Object);

        var result = claimService.GetUserUrn();
        result.Should().Be(userUrn);
    }

    [Fact]
    public void GetUserUrn_ShouldReturnNull_WhenUserHasNoSubClaim()
    {
        var claims = new List<Claim>();
        var httpContextAccessor = GivenHttpContextWith(claims);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.GetUserUrn();

        result.Should().BeNull();
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
