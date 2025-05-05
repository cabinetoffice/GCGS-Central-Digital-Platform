using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsCompanyRegisterNamePage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "label.govuk-label--l";
        private readonly string CompaniesHouseRadioSelector = "input#registerNameCompaniesHouse";
        private readonly string OtherRadioSelector = "input#registerNameOther";
        private readonly string OtherRegisterNameInputSelector = "#RegisterNameInput";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsCompanyRegisterNamePage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl();
        }

        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/company-register-name";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Company Register Name Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task SelectRegisterOption(string option, string? otherValue = null)
        {
            if (option.ToLower() == "companies house")
            {
                await _page.CheckAsync(CompaniesHouseRadioSelector);
                Console.WriteLine("✅ Selected: Companies House");
            }
            else if (option.ToLower() == "other")
            {
                await _page.CheckAsync(OtherRadioSelector);
                await _page.FillAsync(OtherRegisterNameInputSelector, otherValue ?? "");
                Console.WriteLine($"✅ Selected: Other — entered '{otherValue}'");
            }
            else
            {
                throw new System.Exception($"❌ Invalid register option: {option}. Must be 'companies house' or 'other'.");
            }
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        public async Task CompletePage(string option, string? otherValue = null)
        {
            await SelectRegisterOption(option, otherValue);
            await ClickContinue();
            Console.WriteLine("✅ Completed Company Register Name Page.");
        }
    }
}
