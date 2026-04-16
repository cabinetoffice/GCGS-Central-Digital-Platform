using E2ETests.ApiTests;
using E2ETests.Pages.Users;
using E2ETests.Utilities;

namespace E2ETests.Users;

/// <summary>
/// Navigation tests for the UserManagement.App, exercised when the UserManagement feature flag is on.
/// These tests target the separate UserManagement service directly (port 8068 in local dev).
/// </summary>
public class UserManagementNavigationTests : BaseTest
{
    private const string OrgKey = "UserManagement_Org";
    private UserManagementUsersListPage _usersListPage = null!;
    private string _organisationId = string.Empty;

    [SetUp]
    public async Task SetupUserManagementData()
    {
        await base.Setup();

        string accessToken = GetAccessToken();
        await OrganisationApi.CreateOrganisation(accessToken, "UserManagementOrg", OrgKey);
        _organisationId = OrganisationApi.GetOrganisationId(OrgKey);
        Console.WriteLine($"📌 Stored Organisation ID for UserManagement Tests: {_organisationId}");

        _usersListPage = new UserManagementUsersListPage(_page);
    }

    [Test]
    public async Task NavigateToUserManagementOrganisationPage()
    {
        await _usersListPage.NavigateTo(_organisationId);

        var currentUrl = await _usersListPage.GetCurrentUrl();
        Assert.That(currentUrl, Does.Contain($"/organisation/{_organisationId}"),
            "the UserManagement app should serve the users list at /organisation/{guid}");

        TestContext.Out.WriteLine($"✅ Navigated to UserManagement organisation page for {_organisationId}");
    }
}
