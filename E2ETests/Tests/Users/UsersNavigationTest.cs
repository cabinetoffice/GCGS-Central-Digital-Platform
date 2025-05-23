using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.Pages;

namespace E2ETests.Users
{
    public class UsersNavigationTests : UsersBaseTest
    {
        private const string OrgKey = "Users_Org";

        [Test]
        public async Task NavigateToUserSummaryPage()
        {
            await _userSummaryPage.NavigateTo(OrgKey);
            TestContext.WriteLine("✅ Navigated to User Summary Page");
        }
    }
}