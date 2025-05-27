using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationCheckYourAnswersPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "798cf1c1-40be-4e49-9adb-252219d5599d";
    private readonly string _questionId = "5f2ef8a8-c952-4ba4-b833-27ba385a9371";

    private readonly string PageTitleSelector = "h1.govuk-label--l";    
    private readonly string BackLinkSelector = "a.govuk-back-link";
    private readonly string QualificationNameChangeLink = "a[href*='questions/c8f36f11-b4fd-4dfd-9455-bbea0622b2ea?frm-chk-answer=true']";
    private readonly string AwardedByChangeLink = "a[href*='questions/08932077-0f7b-430b-8ce9-679ee6a827cf?frm-chk-answer=true']";    
    private readonly string DateAwardedChangeLink = "a[href*='questions/df2587ef-7bbf-4fe8-8e19-b63526254380?frm-chk-answer=true']";
    private readonly string SubmitButtonSelector = "button:text('Save')";

    public QualificationCheckYourAnswersPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
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
        Console.WriteLine($"📌 Navigated to Check your answers page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task ClickChangeAwardedBy()
    {
        await _page.ClickAsync(AwardedByChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Awarded By");
    }

    public async Task ClickChangeQualificationName()
    {
        await _page.ClickAsync(QualificationNameChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Qualification name");
    }

    public async Task ClickChangeDateAwarded()
    {
        await _page.ClickAsync(DateAwardedChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Date Awarded");
    }

    public async Task ClickSubmit()
    {
        await _page.ClickAsync(SubmitButtonSelector);
        Console.WriteLine("✅ Clicked 'Save' on Check Your Answers Page");
    }
    public async Task ClickBackLink()
    {
        await _page.ClickAsync(BackLinkSelector);
    }

    public async Task CompletePage()
    {
        //await _page.PauseAsync();
        await ClickSubmit();
        Console.WriteLine($"✅ Submitted Qualification Check Your Answers Page");
    }

    public async Task<bool> IsLoaded()
    {
        return await _page.Locator(PageTitleSelector).InnerTextAsync() == "Check your answers";
    }
}