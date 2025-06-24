using Microsoft.Playwright;

namespace E2ETests.Pages.OrganisationDetails;

public class OrganisationName
{
    private readonly IPage _page;

    // ✅ Page Elements
    private const string PageHeadingSelector = "h1.govuk-fieldset__heading";
    private const string SaveButtonSelector = "main >> button[type='submit']";
    private const string OrganisationNameInputSelector = "input#OrganisationName";

    public OrganisationName(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task ChangeOrganisationName(string newOrgName)
    {
        await _page.FillAsync(OrganisationNameInputSelector, newOrgName);
        Console.WriteLine($"✅ Entered organisation name: {newOrgName}");
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(SaveButtonSelector);
        Console.WriteLine("✅ Clicked 'Save' on Organisation Name Page");
    }

    public async Task CompletePage(string newOrgName)
    {
        await ChangeOrganisationName(newOrgName);
        await ClickSave();
        Console.WriteLine("✅ Completed Organisation Name Page");
    }
}
