namespace E2ETests.TradeAssurances;

[Category("TradeAssurances")]
public class TradeAssurancesFunctionalTests : TradeAssurancesBaseTest
{
    [Test]
    public async Task AddTradeAssurancesJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing the full journey of adding a trade assurance.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"TradeAssurance_ {uniqueOrgId}";

        await _tradeAssurancesYesNoPage.NavigateTo("TradeAssurances_Org");
        await _tradeAssurancesYesNoPage.CompletePage(true);
        await _tradeAssurancesAssurancesWhoAwardedPage.CompletePage();
        await _tradeAssurancesKnowReferenceNumberPage.CompletePage();
        await _tradeAssurancesWhenAwardedPage.CompletePage("20", "5", "2007");
        await _tradeAssurancesCheckYourAnswersPage.CompletePage();
        await _tradeAssurancesSummaryPage.AssertTradeAssurancesCount(1);
        await _tradeAssurancesSummaryPage.CompletePage(true);
        await _tradeAssurancesAssurancesWhoAwardedPage.CompletePage();
        await _tradeAssurancesKnowReferenceNumberPage.CompletePage();
        await _tradeAssurancesWhenAwardedPage.CompletePage("28", "3", "2008");
        await _tradeAssurancesCheckYourAnswersPage.CompletePage();
        await _tradeAssurancesSummaryPage.AssertTradeAssurancesCount(2);
    }

}
