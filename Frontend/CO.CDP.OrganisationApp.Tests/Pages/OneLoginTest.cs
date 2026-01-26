using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.Person.WebApiClient;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using System.Security.Claims;
using CO.CDP.Authentication.Services;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OneLoginTest
{
    private readonly Mock<IPersonClient> personClientMock = new();
    private readonly Mock<ISession> sessionMock = new();
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock = new();
    private readonly Mock<ILogoutManager> logoutManagerMock = new();
    private readonly Mock<IOneLoginAuthority> oneLoginAuthorityMock = new();
    private readonly Mock<IAuthenticationService> authService = new();
    private readonly Mock<IAuthorityClient> authorityClientMock = new();
    private readonly Mock<IFeatureManager> featureManagerMock = new();
    private readonly Mock<IConfiguration> configMock = new();
    private const string urn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9";

    [Fact]
    public async Task OnGetSignIn_ShouldReturnAuthChallange()
    {
        var model = GivenOneLoginModel("sign-in");

        var result = await model.OnGetAsync();

        result.Should().BeOfType<ChallengeResult>()
            .Which.Properties!.RedirectUri.Should().Be("/one-login/user-info");

        sessionMock.Verify(s => s.Remove("UserAuthTokens"), Times.Once);
    }

    [Fact]
    public async Task OnGetSignIn_WhenRedirectUrlProvides_ShouldReturnAuthChallangeWithRedirectUrlQueryString()
    {
        var model = GivenOneLoginModel("sign-in");

        var result = await model.OnGetAsync("/org/1");

        result.Should().BeOfType<ChallengeResult>()
            .Which.Properties!.RedirectUri.Should().Be("/one-login/user-info?redirectUri=%2Forg%2F1");
    }

    [Fact]
    public async Task OnGetSignIn_WhenOriginIsNotNull_ShouldSetCorrectRedirectUri()
    {
        configMock.Setup(c => c["FtsServiceAllowedOrigins"]).Returns("http://example1.com,http://example2.com");
        featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AllowDynamicFtsOrigins)).ReturnsAsync(true);
        var origin = "http://example1.com";
        var model = GivenOneLoginModel("sign-in");

        var result = await model.OnGetAsync("/org/1", origin: origin);

        result.As<ChallengeResult>().Properties!.RedirectUri.Should()
            .Be("/one-login/user-info?redirectUri=%2Forg%2F1%3Forigin%3Dhttp%253a%252f%252fexample1.com");
    }

    [Fact]
    public async Task OnGetSignIn_WhenOriginIsNotListedInConfiguration_ShouldSetCorrectRedirectUri()
    {
        configMock.Setup(c => c["FtsServiceAllowedOrigins"]).Returns("http://example1.com,http://example2.com");
        featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AllowDynamicFtsOrigins)).ReturnsAsync(true);
        var origin = "http://example3.com";
        var model = GivenOneLoginModel("sign-in");

        var result = await model.OnGetAsync("/org/1", origin: origin);

        result.As<ChallengeResult>().Properties!.RedirectUri.Should()
            .Be("/one-login/user-info?redirectUri=%2Forg%2F1");
    }

    [Fact]
    public async Task OnGetSignIn_WhenAllowDynamicFtsOriginsFeatureIsDisabled_ShouldNotSetSessionWithFtsServiceOriginKey()
    {
        configMock.Setup(c => c["FtsServiceAllowedOrigins"]).Returns("http://example1.com,http://example2.com");
        featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AllowDynamicFtsOrigins)).ReturnsAsync(false);

        var model = GivenOneLoginModel("sign-in");

        var result = await model.OnGetAsync("/org/1", origin: "http://example1.com");

        sessionMock.Verify(s => s.Set(Session.FtsServiceOrigin, "origin"), Times.Never);
    }

    [Fact]
    public async Task OnGetUserInfo_OnSuccessfulAuthentication_ShouldRetrieveUserProfile()
    {
        var model = GivenOneLoginModel("user-info");

        var result = await model.OnGetAsync();

        sessionMock.Verify(v => v.Set(Session.UserDetailsKey,
            It.Is<UserDetails>(rd =>
                rd.Email == "dummy@test.com"
                && rd.Phone == "+44 123456789"
                && rd.UserUrn == urn
            )), Times.Once);

        logoutManagerMock.Verify(v => v.RemoveAsLoggedOut(urn), Times.Once);
    }

    [Fact]
    public async Task OnGetUserInfo_OnAuthenticationFail_ShouldRedirectToSignIn()
    {
        var model = GivenOneLoginModel("user-info");

        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultFail);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public async Task OnGetUserInfo_OnAuthenticationWithMissingSubject_ShouldRedirectToSignIn()
    {
        var model = GivenOneLoginModel("user-info");

        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultWithMissingSubjectSuccess);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonAlreadyRegistered_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dummyPerson);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonAlreadyRegisteredAndRelativeRedirectUrlProvided_ShouldRedirectToRedirectUrl()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dummyPerson);

        var result = await model.OnGetAsync("/org/1");

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/org/1");
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonAlreadyRegisteredAndAbsoluteRedirectUrlProvided_ShouldRedirectToOrganisationDetailsPageWithoutRedirectUrlQueryString()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dummyPerson);

        var result = await model.OnGetAsync("http://test-domain/org/1");

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonAlreadyRegisteredButReturningWithDifferentUrn_ShouldUpdatePersonRecordWithNewUrn()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync("different urn", null))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        personClientMock.Setup(t => t.LookupPersonAsync(null, dummyPerson.Email))
            .ReturnsAsync(dummyPerson);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Organisation/OrganisationSelection");

        personClientMock.Verify(t => t.UpdatePersonAsync(dummyPerson.Id, It.Is<UpdatedPerson>(p => p.UserUrn == urn)), Times.Once);
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonNotRegistered_ShouldRedirectToPrivacyPolicyPage()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("PrivacyPolicy");
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonNotRegisteredAndRelativeRedirectUrlProvided_ShouldRedirectToPrivacyPolicyPageWithRedirectUrlQueryString()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGetAsync("/org/1");

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>();
        redirectToPageResult.Which.PageName.Should().Be("PrivacyPolicy");
        redirectToPageResult.Which.RouteValues.Should().BeEquivalentTo(new Dictionary<string, string> { { "RedirectUri", "/org/1" } });
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonNotRegisteredAndAbsoluteRedirectUrlProvided_ShouldRedirectToPrivacyPolicyPageWithoutRedirectUrlQueryString()
    {
        var model = GivenOneLoginModel("user-info");

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGetAsync("http://test-domain/org/1");

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>();
        redirectToPageResult.Which.PageName.Should().Be("PrivacyPolicy");
        redirectToPageResult.Which.RouteValues.Should().BeEquivalentTo(new Dictionary<string, string?> { { "RedirectUri", default } });
    }

    [Fact]
    public async Task OnGetSignOut_SessionHasUserDetailsUrn_ShouldCallRevokeRefreshToken()
    {
        var userUrn = "test_urn";
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = userUrn });
        featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AllowFtsRedirectLinks))
            .ReturnsAsync(false);

        var model = GivenOneLoginModel("sign-out");

        var result = await model.OnGetAsync();

        authorityClientMock.Verify(a => a.RevokeRefreshToken(userUrn), Times.Once);
    }

    [Fact]
    public async Task OnGetSignOut_UserIsAuthenticated_ShouldReturnAuthChallange()
    {
        var model = GivenOneLoginModel("sign-out");
        httpContextAccessorMock.Setup(x => x.HttpContext!.User!.Identity!.IsAuthenticated)
           .Returns(true);
        featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AllowFtsRedirectLinks))
            .ReturnsAsync(false);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<SignOutResult>()
            .Which.Properties!.RedirectUri.Should().Be("/user/signedout");
    }

    [Fact]
    public async Task OnGet_UnknownActionParameter_ShouldReturnToIndex()
    {
        var model = GivenOneLoginModel("null");

        var result = await model.OnGetAsync();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/");
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnBadRequest_WhenPageActionIsNotBackChannelSignOut()
    {
        var model = GivenOneLoginModel("some-other-action");

        var result = await model.OnPostAsync("valid-token");

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid page request");
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnBadRequest_WhenLogoutTokenIsEmpty()
    {
        var model = GivenOneLoginModel("back-channel-sign-out");

        var result = await model.OnPostAsync("");

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Missing token");
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnBadRequest_WhenValidateLogoutTokenReturnsNull()
    {
        var model = GivenOneLoginModel("back-channel-sign-out");
        var logoutToken = "invalid-token";
        oneLoginAuthorityMock.Setup(m => m.ValidateLogoutToken(logoutToken)).ReturnsAsync((string?)null);

        var result = await model.OnPostAsync(logoutToken);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid token");
    }

    [Fact]
    public async Task OnPostAsync_ShouldAddUserLoggedOut_WhenValidTokenAndUrn()
    {
        var model = GivenOneLoginModel("back-channel-sign-out");
        var logoutToken = "valid-token";
        var urn = "valid-urn";

        oneLoginAuthorityMock.Setup(m => m.ValidateLogoutToken(logoutToken)).ReturnsAsync(urn);

        var result = await model.OnPostAsync(logoutToken);

        logoutManagerMock.Verify(m => m.MarkAsLoggedOut(urn, logoutToken), Times.Once);
        result.Should().BeOfType<PageResult>();
    }

    private readonly AuthenticateResult authResultSuccess = AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(JwtClaimTypes.Subject, urn),
                    new Claim(JwtClaimTypes.Email, "dummy@test.com"),
                    new Claim(JwtClaimTypes.PhoneNumber, "+44 123456789")
                ])),
            "TestScheme"));

    private readonly AuthenticateResult authResultWithMissingSubjectSuccess = AuthenticateResult.Success(new AuthenticationTicket(
        new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(JwtClaimTypes.Email, "dummy@test.com"),
            new Claim(JwtClaimTypes.PhoneNumber, "+44 123456789")
        ])),
        "TestScheme"));

    private readonly AuthenticateResult authResultFail = AuthenticateResult.Fail(new Exception("Auth failed"));

    private readonly Person.WebApiClient.Person dummyPerson
        = new("dummy@test.com", "firstdummy", new Guid("0bacf3d1-3b69-4efa-80e9-3623f4b7786e"), "lastdummy", new List<string>());

    private OneLoginModel GivenOneLoginModel(string pageAction)
    {
        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultSuccess);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);

        httpContextAccessorMock.SetupGet(t => t.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProvider.Object });

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        return new OneLoginModel(
            httpContextAccessorMock.Object,
            personClientMock.Object,
            sessionMock.Object,
            logoutManagerMock.Object,
            oneLoginAuthorityMock.Object,
            authorityClientMock.Object,
            new Mock<ILogger<OneLoginModel>>().Object,
            featureManagerMock.Object,
            configMock.Object)
        { PageAction = pageAction };
    }
}