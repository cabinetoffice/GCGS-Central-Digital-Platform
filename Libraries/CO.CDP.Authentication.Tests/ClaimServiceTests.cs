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
        var tenValue = "H4sIAAAAAAAAA42QS2/CMBCE/0rlM87DJGD71Fa0KAeoSkKrtkLI2E5wIXZwEh5C/Pc64lCkXqo97czqm9WcwbyWFtAzmFsNKGitprngtDB7r91QFCBEh4ds9zGePL8ei3aTpVWaoLERk4Mkyz0aLd93avZ5TL+zx1MCemDKSulAozWz5d2btCVz4lPJ1NapolOlFpbtOwej+6JzPG5KcOmBTGqmmxrQrzNIhLuPh4LnEeYQkzCAURhgSHgeQo5IP44wZgGOfzMzWTdue7EF06pmjTL6hkUGggd5P4Y8iAmMBCaQhDmDbLBisYzwIGToD2tmtrJjgLStqq1yVS16rirlLtZNU9XU97moPM5WSsvG5Lni0ruW55vbP/x/xqfcVNfEh9EkmYLFpZsfgDXIV6cBAAA=";
        var httpContextAccessor = GivenHttpContextWith([new("ten", tenValue)]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.HaveAccessToOrganisation(new Guid("57dcf48c-8910-4108-9cf1-c2935488a085"));

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveAccessToOrganisation_ShouldReturnTrue_WhenDoesHaveAccessToOrganisation()
    {
        var tenValue = "H4sIAAAAAAAAA42QS2/CMBCE/0rlM87DJGD71Fa0KAeoSkKrtkLI2E5wIXZwEh5C/Pc64lCkXqo97czqm9WcwbyWFtAzmFsNKGitprngtDB7r91QFCBEh4ds9zGePL8ei3aTpVWaoLERk4Mkyz0aLd93avZ5TL+zx1MCemDKSulAozWz5d2btCVz4lPJ1NapolOlFpbtOwej+6JzPG5KcOmBTGqmmxrQrzNIhLuPh4LnEeYQkzCAURhgSHgeQo5IP44wZgGOfzMzWTdue7EF06pmjTL6hkUGggd5P4Y8iAmMBCaQhDmDbLBisYzwIGToD2tmtrJjgLStqq1yVS16rirlLtZNU9XU97moPM5WSsvG5Lni0ruW55vbP/x/xqfcVNfEh9EkmYLFpZsfgDXIV6cBAAA=";
        var httpContextAccessor = GivenHttpContextWith([new("ten", tenValue)]);

        var claimService = new ClaimService(httpContextAccessor.Object);
        var result = claimService.HaveAccessToOrganisation(new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2"));

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