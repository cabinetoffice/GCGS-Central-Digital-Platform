using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.ApiTests;
using E2ETests.Utilities;
using E2ETests.Pages;

namespace E2ETests.TradeAssurances
{
    public class TradeAssurancesBaseTest : BaseTest
    {
        protected static string _organisationId;

        // Page Objects
        protected TradeAssurancesYesNoPage _tradeAssurancesYesNoPage;
        protected TradeAssurancesWhoAwardedPage _tradeAssurancesAssurancesWhoAwardedPage;
        protected TradeAssurancesKnowReferenceNumberPage _tradeAssurancesKnowReferenceNumberPage;
        protected TradeAssurancesWhenAwardedPage _tradeAssurancesWhenAwardedPage;
        protected TradeAssurancesCheckYourAnswersPage _tradeAssurancesCheckYourAnswersPage;
        protected TradeAssurancesSummaryPage _tradeAssurancesSummaryPage;

       [SetUp]
        public async Task SetupTradeAssurancesData()
        {
            await base.Setup();

            string accessToken = GetAccessToken();
            string storageKey = "TradeAssurances_Org";

            await OrganisationApi.CreateOrganisation(accessToken, "TradeAssurancesOrg", storageKey);
            _organisationId = OrganisationApi.GetOrganisationId(storageKey);
            Console.WriteLine($"📌 Stored Organisation ID for TradeAssurances Tests: {_organisationId}");

            _tradeAssurancesYesNoPage = new TradeAssurancesYesNoPage(_page);
            _tradeAssurancesAssurancesWhoAwardedPage = new TradeAssurancesWhoAwardedPage(_page);
            _tradeAssurancesKnowReferenceNumberPage = new TradeAssurancesKnowReferenceNumberPage(_page);
            _tradeAssurancesWhenAwardedPage = new TradeAssurancesWhenAwardedPage(_page);
            _tradeAssurancesCheckYourAnswersPage = new TradeAssurancesCheckYourAnswersPage(_page);
            _tradeAssurancesSummaryPage = new TradeAssurancesSummaryPage(_page);
        }
    }
}
