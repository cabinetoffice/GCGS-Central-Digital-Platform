using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class ConnectedPersonsSupplierHasControlPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string YesRadioSelector = "input[name='ControlledByPersonOrCompany'][value='true']";
    private readonly string NoRadioSelector = "input[name='ControlledByPersonOrCompany'][value='false']";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

    public ConnectedPersonsSupplierHasControlPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    /// Navigates to the Connected Person Influence page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/supplier-has-control";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Connected Person Influence Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task SelectInfluenceOption(bool isInfluenced)
    {
        string radioSelector = isInfluenced ? YesRadioSelector : NoRadioSelector;
        await _page.CheckAsync(radioSelector);
        Console.WriteLine($"✅ Selected Influence Option: {(isInfluenced ? "Yes" : "No")}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task CompletePage(bool isInfluenced)
    {
        await SelectInfluenceOption(isInfluenced);
        await ClickContinue();
    }
}
