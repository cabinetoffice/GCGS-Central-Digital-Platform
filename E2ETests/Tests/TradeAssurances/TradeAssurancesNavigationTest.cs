namespace E2ETests.TradeAssurances;

public class TradeAssurancesNavigationTests : TradeAssurancesBaseTest
{
    private const string OrgKey = "TradeAssurances_Org";

    [Test]
    public async Task NavigateToYesNoPage()
    {
        await _tradeAssurancesYesNoPage.NavigateTo(OrgKey);
        TestContext.Out.WriteLine("✅ Navigated to Yes/No Page");
    }
}
