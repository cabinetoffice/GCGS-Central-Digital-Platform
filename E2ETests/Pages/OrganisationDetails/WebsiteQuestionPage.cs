using Microsoft.Playwright;

namespace E2ETests.Pages;

public class WebsiteQuestionPage
{
    private readonly IPage _page;

    // ✅ Selectors
    private const string PageHeadingSelector = "h1.govuk-heading-l";
    private const string YesRadioSelector = "input#websiteReg";
    private const string NoRadioSelector = "input#websiteReg-2";
    private const string WebsiteAddressInputSelector = "input#websiteAddress";
    private const string SubmitButtonSelector = "main >> button[type='submit']";

    public WebsiteQuestionPage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task SelectHasWebsite(bool hasWebsite)
    {
        var selector = hasWebsite ? YesRadioSelector : NoRadioSelector;
        await _page.ClickAsync(selector);
        Console.WriteLine($"✅ Selected 'Has website': {hasWebsite}");
    }

    public async Task EnterWebsiteAddress(string address)
    {
        await _page.WaitForSelectorAsync(WebsiteAddressInputSelector, new() { State = WaitForSelectorState.Visible });
        await _page.FillAsync(WebsiteAddressInputSelector, address);
        Console.WriteLine($"✅ Entered website address: {address}");
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(SubmitButtonSelector);
        Console.WriteLine("✅ Clicked 'Save' on Website Question Page");
    }

    public async Task CompletePage(bool hasWebsite, string address = "")
    {
        await SelectHasWebsite(hasWebsite);
        if (hasWebsite && !string.IsNullOrWhiteSpace(address))
        {
            await EnterWebsiteAddress(address);
        }

        await ClickSave();
        Console.WriteLine("✅ Completed Website Question Page");
    }
}
