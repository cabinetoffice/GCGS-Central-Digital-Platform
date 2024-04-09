using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ServiceClient;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OneLoginCallbackTest
{
    private readonly Mock<IOneLoginClient> oneLoginClientMock;
    private readonly Mock<ITenantClient> tenantClientMock;
    private readonly Mock<ISession> sessionMock;

    Tenant.WebApiClient.Tenant dummyTenant = new(
                new TenantContactInfo("dummy@test.com", "0123456789"),
                Guid.NewGuid(), "dummy"
            );

    public OneLoginCallbackTest()
    {
        oneLoginClientMock = new Mock<IOneLoginClient>();
        tenantClientMock = new Mock<ITenantClient>();
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public async Task OnGet_ShouldRetrieveUserProfile()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet();

        oneLoginClientMock.Verify(v => v.GetUserInfo(), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenUserProfileIsNull_ShouldThrowExcption()
    {
        var model = GivenOneLoginCallbackModel();

        oneLoginClientMock.Setup(t => t.GetUserInfo())
            .ReturnsAsync(default(UserProfile));

        Func<Task> act = model.OnGet;

        await act.Should().ThrowAsync<Exception>().WithMessage("Unable to retrive user info");
    }

    [Fact]
    public async Task OnGet_WhenTenantIsRegistered_ShouldNotRegisterTenantAgain()
    {
        var model = GivenOneLoginCallbackModel();

        tenantClientMock.Setup(t => t.LookupTenantAsync(It.IsAny<string>()))
            .ReturnsAsync(dummyTenant);

        var results = await model.OnGet();

        tenantClientMock.Verify(v => v.CreateTenantAsync(It.IsAny<NewTenant>()), Times.Never);
    }

    [Fact]
    public async Task OnGet_WhenTenantIsNotRegistered_ShouldRegisterTenant()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet();

        tenantClientMock.Verify(v => v.CreateTenantAsync(It.IsAny<NewTenant>()), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenRegisterTenantFails_ShouldThrowExcption()
    {
        var model = GivenOneLoginCallbackModel();

        tenantClientMock.Setup(t => t.CreateTenantAsync(It.IsAny<NewTenant>()))
            .ThrowsAsync(new ApiException("", 500, null, null, null));

        Func<Task> act = model.OnGet;

        await act.Should().ThrowAsync<Exception>().WithMessage("Unable to create tenant");
    }

    [Fact]
    public async Task OnGet_WhenSuccessfulRegistration_ShouldSetRegistrationDetailsInSessionAndRedirect()
    {
        var model = GivenOneLoginCallbackModel();

        var results = await model.OnGet();

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("YourDetails");
    }

    private OneLoginCallback GivenOneLoginCallbackModel()
    {
        oneLoginClientMock.Setup(t => t.GetUserInfo())
            .ReturnsAsync(new UserProfile
            {
                Email = "dummy",
                EmailVerified = true,
                PhoneNumber = "0123456789",
                PhoneNumberVerified = true,
                UserId = "dummy:user_id"
            });

        tenantClientMock.Setup(t => t.LookupTenantAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApiException("", 404, "", default, null));

        tenantClientMock.Setup(t => t.CreateTenantAsync(It.IsAny<NewTenant>()))
            .ReturnsAsync(dummyTenant);

        return new OneLoginCallback(
            oneLoginClientMock.Object,
            tenantClientMock.Object,
            sessionMock.Object);
    }
}
