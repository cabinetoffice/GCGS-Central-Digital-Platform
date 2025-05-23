using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.ApiTests;
using E2ETests.Utilities;
using E2ETests.Pages;

namespace E2ETests.Users
{
    public class UsersBaseTest : BaseTest
    {
        protected static string _organisationId;

        // Page Objects
        protected UserSummaryPage _userSummaryPage;
        protected AddUserPage _addUserPage;
        protected UserCheckYourAnswersPage _checkYourAnswersPage;

        [SetUp]
        public async Task SetupUsersData()
        {
            await base.Setup();

            string accessToken = GetAccessToken();
            string storageKey = "Users_Org";

            await OrganisationApi.CreateOrganisation(accessToken, "UsersOrg", storageKey);
            _organisationId = OrganisationApi.GetOrganisationId(storageKey);
            Console.WriteLine($"📌 Stored Organisation ID for Users Tests: {_organisationId}");

            // Initialise page objects
            _userSummaryPage = new UserSummaryPage(_page);
            _addUserPage = new AddUserPage(_page);
            _checkYourAnswersPage = new UserCheckYourAnswersPage(_page);
        }
    }
}