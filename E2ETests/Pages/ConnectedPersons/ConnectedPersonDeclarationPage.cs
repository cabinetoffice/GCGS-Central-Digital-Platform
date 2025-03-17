using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonDeclarationPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";
        private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";

        public ConnectedPersonDeclarationPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Person Declaration page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/declaration";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Person Declaration Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        public async Task ClickBackToSupplierInformation()
        {
            await _page.ClickAsync(BackToSupplierInfoSelector);
        }

        public async Task CompletePage()
        {
            await ClickContinue();
        }

    }
}
