namespace E2ETests.Users;

public class UsersFunctionalTests : UsersBaseTest
{
    [Category("Users")]
    [Test]
    public async Task AddMultipleUsersJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Add multiple users to an organisation.");

        await _userSummaryPage.NavigateTo("Users_Org");

        // Add first user
        await _userSummaryPage.CompletePage(addAnotherUser: true);
        await _addUserPage.CompletePage("John", "Johnson", "john@johnson.com", "ADMIN");
        await _checkYourAnswersPage.CompletePage();

        // Now we have 2 users
        await _userSummaryPage.AssertUserCount(2);

        // Add second user
        await _userSummaryPage.CompletePage(addAnotherUser: true);
        await _addUserPage.CompletePage("Sally", "Jones", "sally@jones.com", "EDITOR");
        await _checkYourAnswersPage.CompletePage();

        // Now we have 3 users
        await _userSummaryPage.AssertUserCount(3);
        await _userSummaryPage.AssertUserListed("John Johnson", "john@johnson.com");
        await _userSummaryPage.AssertUserListed("Sally Jones", "sally@jones.com");

        await _userSummaryPage.CompletePage(addAnotherUser: false);
    }
}