using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class ConnectedPersonsOrganisationCategoryPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string RegisteredCompanyRadioSelector = "input[name='ConnectedEntityCategory'][value='RegisteredCompany']";
    private readonly string DirectorRadioSelector = "input[name='ConnectedEntityCategory'][value='DirectorOrTheSameResponsibilities']";
    private readonly string ParentSubsidiaryRadioSelector = "input[name='ConnectedEntityCategory'][value='ParentOrSubsidiaryCompany']";
    private readonly string TakenOverCompanyRadioSelector = "input[name='ConnectedEntityCategory'][value='ACompanyYourOrganisationHasTakenOver']";
    private readonly string OtherInfluenceRadioSelector = "input[name='ConnectedEntityCategory'][value='AnyOtherOrganisationWithSignificantInfluenceOrControl']";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

    public ConnectedPersonsOrganisationCategoryPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    /// Navigates to the Connected Persons Organisation Category selection page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/organisation-category";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Connected Persons Organisation Category Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task SelectOrganisationCategory(string category)
    {
        string radioSelector = category.ToLower() switch
        {
            "registered company" => RegisteredCompanyRadioSelector,
            "director" => DirectorRadioSelector,
            "parent or subsidiary" => ParentSubsidiaryRadioSelector,
            "taken over" => TakenOverCompanyRadioSelector,
            "other influence" => OtherInfluenceRadioSelector,
            _ => throw new System.Exception($"❌ Invalid organisation category: {category}. Must be 'registered company', 'director', 'parent or subsidiary', 'taken over', or 'other influence'.")
        };

        await _page.CheckAsync(radioSelector);
        Console.WriteLine($"✅ Selected Organisation Category: {category}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    /// Completes the form by selecting an organisation category and continuing.
    public async Task CompletePage(string category)
    {
        await SelectOrganisationCategory(category);
        await ClickContinue();
        Console.WriteLine($"✅ Completed Organisation Category Page with selection: {category}");
    }
}
