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
    /// Reads the newly created API key from the confirmation page we were redirected to
    /// (the key is no longer in the URL, so we cannot navigate to this page directly) and stores it.
    public async Task CaptureApiKey(string apiKeyName)
    {
        string apiKeyId = await GetApiKeyId();
        StorageUtility.Store(apiKeyName, apiKeyId);
        Console.WriteLine($"📌 Captured API key for '{apiKeyName}' from confirmation page: {_page.Url}");
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