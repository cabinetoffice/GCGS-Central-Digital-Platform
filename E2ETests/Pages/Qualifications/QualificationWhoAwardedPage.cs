using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationWhoAwardedPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "798cf1c1-40be-4e49-9adb-252219d5599d";
    private readonly string _questionId = "08932077-0f7b-430b-8ce9-679ee6a827cf";

    private readonly string PageTitleSelector = "h1.govuk-label--l";
    private readonly string ContinueButtonSelector = "button:text('Continue')";
    private readonly string BackLinkSelector = "a.govuk-back-link";
    private readonly string InputSelector = "input[name='TextInput'][type='text']";

    public QualificationWhoAwardedPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/questions/{_questionId}";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Who awarded the qualification? page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task EnterQualificationName()
    {
        var whoAwarded = "ISO";
        await _page.FillAsync(InputSelector, whoAwarded);
        Console.WriteLine($"✅ Entered Who awarded the qualification: {whoAwarded}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task ClickBackLink()
    {
        await _page.ClickAsync(BackLinkSelector);
    }
    public async Task CompletePage()
    {
        await EnterQualificationName();
        await ClickContinue();
    }
}