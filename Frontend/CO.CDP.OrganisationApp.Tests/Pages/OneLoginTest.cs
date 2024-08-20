using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.Person.WebApiClient;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OneLoginTest
{
    private readonly Mock<IPersonClient> personClientMock;
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly Mock<IAuthenticationService> authService;

    public OneLoginTest()
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        personClientMock = new Mock<IPersonClient>();
        sessionMock = new Mock<ISession>();
        authService = new Mock<IAuthenticationService>();
    }

    [Fact]
    public async Task OnGetSignIn_ShouldReturnAuthChallange()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("sign-in");

        results.Should().BeOfType<ChallengeResult>()
            .Which.Properties!.RedirectUri.Should().Be("/one-login/user-info");
    }

    [Fact]
    public async Task OnGetUserInfo_OnSuccessfulAuthentication_ShouldRetrieveUserProfile()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("user-info");

        sessionMock.Verify(v => v.Set(Session.UserDetailsKey,
            It.Is<UserDetails>(rd =>
                rd.Email == "dummy@test.com"
                && rd.Phone == "+44 123456789"
                && rd.UserUrn == "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9"
            )), Times.Once);
    }

    [Fact]
    public async Task OnGetUserInfo_OnAuthenticationFail_ShouldRedirectToSignIn()
    {
        var model = GivenOneLoginCallbackModel();

        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultFail);

        var results = await model.OnGet("user-info");

        results.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public async Task OnGetUserInfo_OnAuthenticationWithMissingSubject_ShouldRedirectToSignIn()
    {
        var model = GivenOneLoginCallbackModel();

        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultWithMissingSubjectSuccess);

        var results = await model.OnGet("user-info");

        results.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonAlreadyRegistered_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenOneLoginCallbackModel();

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>()))
            .ReturnsAsync(dummyPerson);

        var results = await model.OnGet("user-info");

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationSelection");
    }

    [Fact]
    public async Task OnGetUserInfo_WhenPersonNotRegistered_ShouldRedirectToPrivacyPolicyPage()
    {
        var model = GivenOneLoginCallbackModel();

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var results = await model.OnGet("user-info");

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("PrivacyPolicy");
    }

    [Fact]
    public async Task OnGetSignOut_UserIsNotAuthenticated_ShouldReturnToIndex()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("sign-out");

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/");
    }

    [Fact]
    public async Task OnGetSignOut_UserIsAuthenticated_ShouldReturnAuthChallange()
    {
        var model = GivenOneLoginCallbackModel();

        httpContextAccessorMock.Setup(x => x.HttpContext!.User!.Identity!.IsAuthenticated)
           .Returns(true);

        var results = await model.OnGet("sign-out");

        results.Should().BeOfType<SignOutResult>()
            .Which.Properties!.RedirectUri.Should().Be("/");
    }

    [Fact]
    public async Task OnGet_UnknownActionParameter_ShouldReturnToIndex()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("null");

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/");
    }

    private readonly AuthenticateResult authResultSuccess = AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(JwtClaimTypes.Subject, "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9"),
                    new Claim(JwtClaimTypes.Email, "dummy@test.com"),
                    new Claim(JwtClaimTypes.PhoneNumber, "+44 123456789")
                })),
            "TestScheme"));

    private readonly AuthenticateResult authResultWithMissingSubjectSuccess = AuthenticateResult.Success(new AuthenticationTicket(
        new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(JwtClaimTypes.Email, "dummy@test.com"),
            new Claim(JwtClaimTypes.PhoneNumber, "+44 123456789")
        })),
        "TestScheme"));

    private readonly AuthenticateResult authResultFail = AuthenticateResult.Fail(new Exception("Auth failed"));

    private readonly Person.WebApiClient.Person dummyPerson
        = new("dummy@test.com", "firstdummy", new Guid("0bacf3d1-3b69-4efa-80e9-3623f4b7786e"), "lastdummy");

    private OneLogin GivenOneLoginCallbackModel()
    {
        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultSuccess);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);

        httpContextAccessorMock.SetupGet(t => t.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProvider.Object });

        personClientMock.Setup(t => t.LookupPersonAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        return new OneLogin(
            httpContextAccessorMock.Object,
            personClientMock.Object,
            sessionMock.Object);
    }
}