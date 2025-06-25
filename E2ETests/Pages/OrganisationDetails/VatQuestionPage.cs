using Microsoft.Playwright;

namespace E2ETests.Pages;

public class VatQuestionPage
{
    private readonly IPage _page;

    // ✅ Page Locators
    private const string PageHeadingSelector = "h1.govuk-heading-l";
    private const string YesRadioSelector = "input#vatReg";
    private const string NoRadioSelector = "input#vatReg-2";
    private const string VatNumberInputSelector = "input#VatNumber";
    private const string SubmitButtonSelector = "main >> button[type='submit']";

    public VatQuestionPage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task SelectHasVatNumber(bool hasVatNumber)
    {
        var selector = hasVatNumber ? YesRadioSelector : NoRadioSelector;
        await _page.ClickAsync(selector);
        Console.WriteLine($"✅ Selected VAT registered: {hasVatNumber}");
    }

    public async Task EnterVatNumber(string vatNumber)
    {
        // Ensure the conditional input is shown
        await _page.WaitForSelectorAsync(VatNumberInputSelector, new() { State = WaitForSelectorState.Visible });
        await _page.FillAsync(VatNumberInputSelector, vatNumber);
        Console.WriteLine($"✅ Entered VAT number: {vatNumber}");
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(SubmitButtonSelector);
        Console.WriteLine("✅ Clicked 'Save' on VAT Question Page");
    }

    public async Task CompletePage(bool hasVatNumber, string vatNumber = "")
    {
        await SelectHasVatNumber(hasVatNumber);
        if (hasVatNumber && !string.IsNullOrWhiteSpace(vatNumber))
        {
            await EnterVatNumber(vatNumber);
        }

        await ClickSave();
        Console.WriteLine("✅ Completed VAT Question Page");
    }
}
