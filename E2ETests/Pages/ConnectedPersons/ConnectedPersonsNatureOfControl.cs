using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;
using System.Collections.Generic;

namespace E2ETests.Pages
{
    public class ConnectedPersonsNatureOfControl
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string OwnsSharesCheckBoxSelector = "input[name='ControlConditions'][value='OwnsShares']";
        private readonly string HasVotingRightsCheckBoxSelector = "input[name='ControlConditions'][value='HasVotingRights']";
        private readonly string CanAppointOrRemoveDirectorsCheckBoxSelector = "input[name='ControlConditions'][value='CanAppointOrRemoveDirectors']";
        private readonly string HasOtherSignificantInfluenceOrControlCheckBoxSelector = "input[name='ControlConditions'][value='HasOtherSignificantInfluenceOrControl']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsNatureOfControl(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/nature-of-control";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Nature of Control Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// ✅ Accepts a list of checkbox labels to select (e.g., ["owns shares", "has voting rights"])
        public async Task SelectControlOptions(List<string> options)
        {
            foreach (var option in options)
            {
                string selector = option.ToLower() switch
                {
                    "owns shares" => OwnsSharesCheckBoxSelector,
                    "has voting rights" => HasVotingRightsCheckBoxSelector,
                    "can appoint or remove directors" => CanAppointOrRemoveDirectorsCheckBoxSelector,
                    "other influence" => HasOtherSignificantInfluenceOrControlCheckBoxSelector,
                    _ => throw new System.Exception($"❌ Invalid control option: {option}")
                };

                await _page.CheckAsync(selector);
                Console.WriteLine($"✅ Checked control option: {option}");
            }
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        public async Task CompletePage(List<string> options)
        {
            await SelectControlOptions(options);
            await ClickContinue();
            Console.WriteLine("✅ Completed Nature of Control Page.");
        }
    }
}
