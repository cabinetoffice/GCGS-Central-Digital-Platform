using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.Tenant.WebApiClient;
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
    private readonly Mock<ITenantClient> tenantClientMock;
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly Mock<IAuthenticationService> authService;

    public OneLoginTest()
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        tenantClientMock = new Mock<ITenantClient>();
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

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey,
            It.Is<RegistrationDetails>(rd =>
                rd.Email == "dummy@test.com"
                && rd.TenantId == new Guid("0bacf3d1-3b69-4efa-80e9-3623f4b7786e"))), Times.Once);
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
    public async Task OnGetUserInfo_WhenTenantIsRegistered_ShouldNotRegisterTenantAgain()
    {
        var model = GivenOneLoginCallbackModel();

        tenantClientMock.Setup(t => t.LookupTenantAsync(It.IsAny<string>()))
            .ReturnsAsync(dummyTenant);

        var results = await model.OnGet("user-info");

        tenantClientMock.Verify(v => v.CreateTenantAsync(It.IsAny<NewTenant>()), Times.Never);
    }

    [Fact]
    public async Task OnGetUserInfo_WhenTenantIsNotRegistered_ShouldRegisterTenant()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("user-info");

        tenantClientMock.Verify(v => v.CreateTenantAsync(It.IsAny<NewTenant>()), Times.Once);
    }

    [Fact]
    public async Task OnGetUserInfo_WhenSuccessfulRegistration_ShouldSetRegistrationDetailsInSessionAndRedirect()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet("user-info");

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Registration/YourDetails");
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

    private readonly AuthenticateResult authResultFail = AuthenticateResult.Fail(new Exception("Auth failed"));

    private Tenant.WebApiClient.Tenant dummyTenant = new(
                new TenantContactInfo("dummy@test.com", "0123456789"),
                new Guid("0bacf3d1-3b69-4efa-80e9-3623f4b7786e"), "dummy"
            );

    private OneLogin GivenOneLoginCallbackModel()
    {
        authService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResultSuccess);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);

        httpContextAccessorMock.SetupGet(t => t.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProvider.Object });

        tenantClientMock.Setup(t => t.LookupTenantAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        tenantClientMock.Setup(t => t.CreateTenantAsync(It.IsAny<NewTenant>()))
            .ReturnsAsync(dummyTenant);

        return new OneLogin(
            httpContextAccessorMock.Object,
            tenantClientMock.Object,
            sessionMock.Object);
    }
}
