namespace E2ETests.Users;

public class UsersNavigationTests : UsersBaseTest
{
    private const string OrgKey = "Users_Org";

    [Test]
    [Ignore("Disabled: functionality moved behind UserManagement feature flag — to be re-enabled in follow-up PR")]
    public async Task NavigateToUserSummaryPage()
    {
        await _userSummaryPage.NavigateTo(OrgKey);
        TestContext.Out.WriteLine("✅ Navigated to User Summary Page");
    }
}