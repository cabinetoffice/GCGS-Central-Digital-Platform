using Microsoft.Playwright;

namespace E2ETests.Pages;

public class OrganisationTypePage
{
    private readonly IPage _page;

    // ✅ Page Elements
    private const string PageHeadingSelector = "h1.govuk-fieldset__heading";
    private const string SaveButtonSelector = "main >> button[type='submit']";

    // Checkbox selectors
    private const string SMECheckbox = "input#chkSmallOrMediumAized";
    private const string VCSECheckbox = "input#chkNonGovernmental";
    private const string SupportedEmploymentCheckbox = "input#chkSupportedEmploymentProvider";
    private const string PublicServiceMutualCheckbox = "input#chkPublicService";
    private const string NoneApplyCheckbox = "input#chkNoneOfTheAbove";

    public OrganisationTypePage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task SelectOrganisationTypes(params string[] types)
    {
        var typeMap = new Dictionary<string, string>
        {
            ["SME"] = SMECheckbox,
            ["VCSE"] = VCSECheckbox,
            ["SupportedEmployment"] = SupportedEmploymentCheckbox,
            ["PublicService"] = PublicServiceMutualCheckbox,
            ["None"] = NoneApplyCheckbox
        };

        foreach (var type in types)
        {
            if (typeMap.TryGetValue(type, out var selector))
            {
                await _page.CheckAsync(selector);
                System.Console.WriteLine($"✅ Selected organisation type: {type}");
            }
            else
            {
                throw new KeyNotFoundException($"❌ Unknown organisation type: {type}");
            }
        }
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(SaveButtonSelector);
        System.Console.WriteLine("✅ Clicked 'Save' on Organisation Type Page");
    }

    public async Task CompletePage(params string[] types)
    {
        await SelectOrganisationTypes(types);
        await ClickSave();
        System.Console.WriteLine("✅ Completed Organisation Type Page");
    }
}
