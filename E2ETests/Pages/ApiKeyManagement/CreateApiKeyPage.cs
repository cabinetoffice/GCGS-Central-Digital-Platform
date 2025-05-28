using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.ApiKeyManagement;
public class CreateApiKeyPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-label--l";
    private readonly string CreateApiKeyButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string InputSelector = "#ApiKeyName";
    public CreateApiKeyPage(IPage page)
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
        Console.WriteLine("✅ Clicked Create API Key Button");
    }
    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
        Console.WriteLine("✅ Clicked Back to Supplier Information");
    }
    public async Task EnterApiKeyName(string apiKeyName)
    {
        await _page.FillAsync(InputSelector, apiKeyName);
        Console.WriteLine($"✅ Entered API Key Name: {apiKeyName}");
    }
    public async Task CompletePage(string apiKeyName)
    {
        await EnterApiKeyName(apiKeyName);
        await ClickCreateApiKey();
        Console.WriteLine($"✅ Completed API Key Page with name: {apiKeyName}");
    }
}