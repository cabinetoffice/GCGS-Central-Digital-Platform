using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class ConnectedPersonsCompanyQuestionPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string YesRadioSelector = "input[name='HasCompaniesHouseNumber'][value='true']";
    private readonly string NoRadioSelector = "input[name='HasCompaniesHouseNumber'][value='false']";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

    public ConnectedPersonsCompanyQuestionPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    /// Navigates to the Connected Persons Company Question Page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/company-question";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Connected Persons Company Question Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task SelectCompanyRegistered(bool isRegistered)
    {
        string optionSelector = isRegistered ? YesRadioSelector : NoRadioSelector;
        await _page.CheckAsync(optionSelector);
        Console.WriteLine($"✅ Selected Company Registered: {(isRegistered ? "Yes" : "No")}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    /// Completes the form by selecting an option and clicking continue.
    public async Task CompletePage(bool isRegistered)
    {
        await SelectCompanyRegistered(isRegistered);
        await ClickContinue();
        Console.WriteLine($"✅ Completed Company Question Page with selection: {(isRegistered ? "Yes" : "No")}");
    }
}
