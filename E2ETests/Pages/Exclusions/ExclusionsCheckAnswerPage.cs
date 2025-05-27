using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionsCheckAnswerPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "7c105936-607a-46e0-97e6-5f1421e11b0f";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string SaveButtonSelector = "button.govuk-button[type='submit']";

    private readonly string UKexclusionChangeLinkSelector = "UKexclusion_1";
    private readonly string ExclusionappliesChangeLinkSelector = "Exclusionapplies_2";
    private readonly string ExclusionappliestoChangeLinkSelector = "Exclusionappliesto_3";
    private readonly string ContactemailLinkChangeSelector = "Contactemail_4";
    private readonly string ExclusionindetailChangeLinkSelector = "Exclusionindetail_5";
    private readonly string ExclusionbeingmanagedChangeLinkSelector = "Exclusionbeingmanaged_6";
    private readonly string SupportingdocumentChangeLinkSelector = "Supportingdocument_7";
    private readonly string RecordedonawebsiteChangeLinkSelector = "Recordedonawebsite_8";
    private readonly string DatecircumstancesendedChangeLinkSelector = "Datecircumstancesended_9";

    public ExclusionsCheckAnswerPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/questions/{_questionId}";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Did this exclusion happen in the UK? Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task ClickChangeUKexclusion()
    {
        await _page.GetByTestId(UKexclusionChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for UK exclusion");
    }
    public async Task ClickChangeExclusionapplies()
    {
        await _page.GetByTestId(ExclusionappliesChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Exclusion applies");
    }

    public async Task ClickChangeExclusionappliesto()
    {
        await _page.GetByTestId(ExclusionappliestoChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Exclusion applies to");
    }
    public async Task ClickChangeContactemail()
    {
        await _page.GetByTestId(ContactemailLinkChangeSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Contact email");
    }
    public async Task ClickChangeExclusionindetail()
    {
        await _page.GetByTestId(ExclusionindetailChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Exclusion in detail");
    }
    public async Task ClickChangeExclusionbeingmanaged()
    {
        await _page.GetByTestId(ExclusionbeingmanagedChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Exclusion being managed");
    }
    public async Task ClickChangeSupportingdocument()
    {
        await _page.GetByTestId(SupportingdocumentChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Supporting document");
    }
    public async Task ClickChangeRecordedonawebsite()
    {
        await _page.GetByTestId(RecordedonawebsiteChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Recorded on a website");
    }
    public async Task ClickChangeDatecircumstancesended()
    {
        await _page.GetByTestId(DatecircumstancesendedChangeLinkSelector).ClickAsync();
        Console.WriteLine("✅ Clicked 'Change' for Date circumstances ended");
    }
    public async Task ClickSave()
    {
        await _page.ClickAsync(SaveButtonSelector);
    }

    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
    }

    public async Task CompletePage()
    {
        await ClickSave();
        Console.WriteLine($"✅ Submitted Exclusion Check Your Answers Page");
    }
    public async Task<bool> IsLoaded()
    {
        return await _page.Locator(PageTitleSelector).InnerTextAsync() == "Check your answers";
    }
}