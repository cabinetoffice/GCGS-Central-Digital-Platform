using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.ApiKeyManagement;
public class ManageApiKeyPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string CreateApiKeyButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string CancelApiKeySelector = "cancelLink";
    public ManageApiKeyPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }
    /// Navigates to the Manage API Key page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }
        string url = $"{_baseUrl}/organisation/{organisationId}/manage-api-keys";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Manage API Key Page: {url}");
        await _page.WaitForSelectorAsync(PageTitleSelector);
    }
    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task ClickCreateApiKey()
    {
        await _page.ClickAsync(CreateApiKeyButtonSelector);
        await _page.ClickAsync(CreateApiKeyButtonSelector);
        Console.WriteLine("✅ Clicked Create API Key Button");
    }
    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
        Console.WriteLine("✅ Clicked Back to Supplier Information");
    }
    public async Task CompletePage()
    {
        await ClickCreateApiKey();
    }
    public async Task ClickCancelApiKey()
    {
        await _page.GetByTestId(CancelApiKeySelector).ClickAsync();
        Console.WriteLine("✅ Clicked Cancel API Key link");
    }
    public async Task AssertApiKeyCount(int expectedCount)
    {
        string expectedText = $"You have {expectedCount} API key{(expectedCount == 1 ? "" : "s")}";

        string selector = "h1.govuk-heading-l";

        await _page.WaitForSelectorAsync(selector);
        string actualText = await _page.InnerTextAsync(selector);

        if (actualText.Trim() != expectedText)
        {
            throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
        }

        Console.WriteLine($"✅ API key count verified: {actualText}");
    }
}