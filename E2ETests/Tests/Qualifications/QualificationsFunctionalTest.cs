namespace E2ETests.Tests.Qualifications;

public class QualificationsFunctionalTest : QualificationsBaseTest
{
    [Test]
    public async Task AddQualificationJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing the full journey of adding a general qualification.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"Qualification_ {uniqueOrgId}";

        await _qualificationsYesNoPage.NavigateTo("Qualification_Org");
        await _qualificationsYesNoPage.CompletePage(true);
        await _qualificationNamePage.CompletePage();
        await _qualificationWhoAwardedPage.CompletePage();
        await _qualificationWhenAwardedPage.CompletePage("15", "1", "2012");
        await _qualificationCheckYourAnswersPage.CompletePage();
        await _qualificationsSummaryPage.AssertQualificationCount(1);
    }

    [Test]
    public async Task AddMultipleQualificationJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing the full journey of adding a general qualification.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"Qualification_ {uniqueOrgId}";

        await _qualificationsYesNoPage.NavigateTo("Qualification_Org");
        await _qualificationsYesNoPage.CompletePage(true);
        await _qualificationNamePage.CompletePage();
        await _qualificationWhoAwardedPage.CompletePage();
        await _qualificationWhenAwardedPage.CompletePage("15", "1", "2012");
        await _qualificationCheckYourAnswersPage.CompletePage();
        await _qualificationsSummaryPage.AssertQualificationCount(1);
        await _qualificationsYesNoPage.NavigateTo("Qualification_Org");
        await _qualificationsYesNoPage.CompletePage(true);
        await _qualificationNamePage.CompletePage();
        await _qualificationWhoAwardedPage.CompletePage();
        await _qualificationWhenAwardedPage.CompletePage("15", "12", "2012");
        await _qualificationCheckYourAnswersPage.CompletePage();
        await _qualificationsSummaryPage.AssertQualificationCount(2);
    }

}