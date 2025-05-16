using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.ApiKeyManagement;
public class NewApiKeyDetailsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    // ✅ Page Locators
    private readonly string PageTitleSelector = "#apikeytext";
    private readonly string CreateApiKeyButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToManageApiKeySelector = "backtoManageApiKey";
    public NewApiKeyDetailsPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }
    /// Navigates to the Manage API Key page using stored Organisation ID.
    public async Task NavigateTo(string storageKey, string apiKeyName)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }
        string apiKeyId = await GetApiKeyId();
        string url = $"{_baseUrl}/organisation/{organisationId}/manage-api-key/{apiKeyId}/details";
        StorageUtility.Store(apiKeyName, apiKeyId);
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Manage API Key Page: {url}");
        await _page.WaitForSelectorAsync(PageTitleSelector);
    }
    public async Task<string> GetApiKeyId()
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
    public async Task ClickBackToManageApiKeys()
    {
        await _page.GetByTestId(BackToManageApiKeySelector).ClickAsync();
        Console.WriteLine("✅ Clicked Back to Manage API Keys");
    }    
    public async Task CompletePage()
    {
        await ClickCreateApiKey();
    }
}