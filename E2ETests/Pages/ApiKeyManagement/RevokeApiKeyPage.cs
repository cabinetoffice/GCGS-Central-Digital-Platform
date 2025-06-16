using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.ApiKeyManagement;
public class RevokeApiKeyPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string CancelApiKeyButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    public RevokeApiKeyPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }
    /// Navigates to the Revoke API Key page using stored Organisation ID.
    public async Task NavigateTo(string storageKey, string apiKeyName)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }
        string apiKeyid = StorageUtility.Retrieve(apiKeyName);
        string url = $"{_baseUrl}/organisation/{organisationId}/manage-api-key/{apiKeyName}/revoke-api-key";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Revoke API Key Page: {url}");
        await _page.WaitForSelectorAsync(PageTitleSelector);
    }
    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task ClickCancelApiKey()
    {
        await _page.ClickAsync(CancelApiKeyButtonSelector);
        Console.WriteLine("✅ Clicked Cancel API Key Button");        
    }
    public async Task ClickBackToManageApiKeys()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
        Console.WriteLine("✅ Clicked Back to Manage API Key");
    }

    public async Task CompletePage()
    {
        await ClickCancelApiKey();
    }
}