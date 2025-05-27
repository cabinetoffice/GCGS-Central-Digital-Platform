namespace E2ETests.Tests.Exclusions;

public class ExclusionsFunctionalTest : ExclusionsBaseTest
{
    [Test]
    public async Task AddExclusionJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing the full journey of adding a Exclusion.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"Exclusions_ {uniqueOrgId}";

        await _exclusionsDeclarationPage.NavigateTo("Exclusions_Org");
        await _exclusionsDeclarationPage.CompletePage();
        await _addExclusionsYesNoPage.CompletePage(true);
        await _exclusionsInUKYesNoPage.CompletePage(true);
        await _selectExclusionPage.CompletePage();
        await _selectExclusionAppliesToPage.CompletePage();
        await _exclusionsEnterEmailPage.CompletePage();
        await _exclusionsDescribeInDetailsPage.CompletePage();
        await _exclusionsHowBeingManagedPage.CompletePage();
        await _exclusionsSupportingDocumentsYesNoPage.CompletePage(false);
        await _exclusionsDecisionRecordedYesNoPage.CompletePage(false);
        await _exclusionsEndedYesNoPage.CompletePage(false);
        await _exclusionsCheckAnswerPage.CompletePage();
        await _exclusionSummaryPage.AssertExclusionsCount(1);

        await _exclusionSummaryPage.CompletePage(true);
        await _exclusionsInUKYesNoPage.CompletePage(false);
        await _selectExclusionPage.CompletePage();
        await _selectExclusionAppliesToPage.CompletePage();
        await _exclusionsEnterEmailPage.CompletePage();
        await _exclusionsDescribeInDetailsPage.CompletePage();
        await _exclusionsHowBeingManagedPage.CompletePage();
        await _exclusionsSupportingDocumentsYesNoPage.CompletePage(true);
        await _exclusionsDecisionRecordedYesNoPage.CompletePage(true);
        await _exclusionsEndedYesNoPage.CompletePage(true);
        await _exclusionsCheckAnswerPage.CompletePage();
        await _exclusionSummaryPage.AssertExclusionsCount(2);

    }
}