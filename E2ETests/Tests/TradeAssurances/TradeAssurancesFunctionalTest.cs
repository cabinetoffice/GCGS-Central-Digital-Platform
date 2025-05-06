using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.Pages;

namespace E2ETests.TradeAssurances
{
    [Category("TradeAssurances")]
    public class TradeAssurancesFunctionalTests : TradeAssurancesBaseTest
    {
        [Test]
        public async Task AddTradeAssurancesJourneyHappyPath()
        {
            TestContext.WriteLine("🔹 Scenario: Completing the full journey of adding a trade assurance.");

            string uniqueOrgId = Guid.NewGuid().ToString();
            string uniqueOrgName = $"TradeAssurance_ {uniqueOrgId}";

            await _tradeAssurancesYesNoPage.NavigateTo("TradeAssurances_Org");
            await _tradeAssurancesYesNoPage.CompletePage();
        }

    }
}
