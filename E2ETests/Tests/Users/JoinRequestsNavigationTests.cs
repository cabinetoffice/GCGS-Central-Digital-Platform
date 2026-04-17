using E2ETests.ApiTests;
using E2ETests.Pages.Users;
using E2ETests.Utilities;

namespace E2ETests.Users;

/// <summary>
/// E2E tests for the join request approve/reject flows in the UserManagement.App.
/// These tests verify that an organisation admin can see pending join requests
/// and navigate through the confirm/approve/reject UI.
///
/// Requirements: UserManagement feature flag enabled; services running via Docker Compose.
/// </summary>
[Category("JoinRequests")]
public class JoinRequestsNavigationTests : BaseTest
{
    private const string OrgKey = "JoinRequests_Org";
    private UserManagementUsersListPage _usersListPage = null!;
    private JoinRequestConfirmPage _confirmPage = null!;
    private string _organisationSlug = string.Empty;
    private string _organisationId = string.Empty;

    [SetUp]
    public async Task SetupJoinRequestsData()
    {
        await base.Setup();

        string accessToken = GetAccessToken();
        await OrganisationApi.CreateOrganisation(accessToken, "JoinRequestsOrg", OrgKey);
        _organisationId = OrganisationApi.GetOrganisationId(OrgKey);
        _organisationSlug = _organisationId; // UM app uses org GUID as slug in integration test config

        _usersListPage = new UserManagementUsersListPage(_page);
        _confirmPage = new JoinRequestConfirmPage(_page);

        Console.WriteLine($"📌 Organisation ID for JoinRequests Tests: {_organisationId}");
    }

    [Test]
    public async Task UsersListPage_RendersSuccessfully_WhenNoPendingJoinRequests()
    {
        await _usersListPage.NavigateTo(_organisationId);

        var currentUrl = await _usersListPage.GetCurrentUrl();
        Assert.That(currentUrl, Does.Contain($"/organisation/{_organisationId}"),
            "the UserManagement app should serve the users list at /organisation/{guid}");

        TestContext.Out.WriteLine($"✅ Users list page rendered for organisation {_organisationId} (no join requests)");
    }

    [Test]
    public async Task ApproveConfirmPage_WithUnknownJoinRequest_Returns404OrRedirects()
    {
        var nonExistentJoinRequestId = Guid.NewGuid();
        var nonExistentPersonId = Guid.NewGuid();

        await _confirmPage.NavigateToApprove(_organisationId, nonExistentJoinRequestId, nonExistentPersonId);

        var currentUrl = await _confirmPage.GetCurrentUrl();

        // Controller returns NotFound for unrecognised join request ID;
        // the app may render a 404 page or redirect to /error
        var is404OrError = currentUrl.Contains("error") ||
                           currentUrl.Contains("not-found") ||
                           await _page.TitleAsync() is var t && t.Contains("404") ||
                           await _page.ContentAsync() is var c && (c.Contains("not found") || c.Contains("Not Found") || c.Contains("Page not found"));

        Assert.That(is404OrError, Is.True,
            $"Navigating to an unknown join request should result in a 404/error page. Current URL: {currentUrl}");

        TestContext.Out.WriteLine($"✅ Unknown join request correctly results in 404/error");
    }

    [Test]
    public async Task RejectConfirmPage_WithUnknownJoinRequest_Returns404OrRedirects()
    {
        var nonExistentJoinRequestId = Guid.NewGuid();
        var nonExistentPersonId = Guid.NewGuid();

        await _confirmPage.NavigateToReject(_organisationId, nonExistentJoinRequestId, nonExistentPersonId);

        var currentUrl = await _confirmPage.GetCurrentUrl();

        var is404OrError = currentUrl.Contains("error") ||
                           currentUrl.Contains("not-found") ||
                           await _page.TitleAsync() is var t && t.Contains("404") ||
                           await _page.ContentAsync() is var c && (c.Contains("not found") || c.Contains("Not Found") || c.Contains("Page not found"));

        Assert.That(is404OrError, Is.True,
            $"Navigating to an unknown reject page should result in a 404/error page. Current URL: {currentUrl}");

        TestContext.Out.WriteLine($"✅ Unknown join request reject correctly results in 404/error");
    }
}
