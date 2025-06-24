using Microsoft.Playwright;

namespace E2ETests.Pages;

public class AddUserPage
{
    private readonly IPage _page;

    // ✅ Page Locators
    private readonly string FirstNameInputSelector = "#FirstName";
    private readonly string LastNameInputSelector = "#LastName";
    private readonly string EmailInputSelector = "#Email";
    private readonly string AdminRoleRadioSelector = "input[name='role'][value='ADMIN']";
    private readonly string EditorRoleRadioSelector = "input[name='role'][value='EDITOR']";
    private readonly string ViewerRoleRadioSelector = "input[name='role'][value='VIEWER']";
    private readonly string ContinueButtonSelector = "button:has-text('Continue')";
    private readonly string BackLinkSelector = "a.govuk-back-link";
    private readonly string PageHeadingSelector = "h1.govuk-heading-l";

    public AddUserPage(IPage page)
    {
        _page = page;
    }

    public async Task EnterFirstName(string firstName)
    {
        await _page.FillAsync(FirstNameInputSelector, firstName);
    }

    public async Task EnterLastName(string lastName)
    {
        await _page.FillAsync(LastNameInputSelector, lastName);
    }

    public async Task EnterEmail(string email)
    {
        await _page.FillAsync(EmailInputSelector, email);
    }

    public async Task SelectRole(string role)
    {
        string selector = role.ToUpper() switch
        {
            "ADMIN" => AdminRoleRadioSelector,
            "EDITOR" => EditorRoleRadioSelector,
            "VIEWER" => ViewerRoleRadioSelector,
            _ => throw new System.ArgumentException($"❌ Unknown role: {role}")
        };

        await _page.ClickAsync(selector);
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
        Console.WriteLine("✅ Clicked 'Continue' on Add User Page");
    }

    public async Task ClickBack()
    {
        await _page.ClickAsync(BackLinkSelector);
        Console.WriteLine("✅ Clicked 'Back' on Add User Page");
    }

    public async Task<string> GetHeadingText()
    {
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task CompletePage(string firstName, string lastName, string email, string role)
    {
        await EnterFirstName(firstName);
        await EnterLastName(lastName);
        await EnterEmail(email);
        await SelectRole(role);
        await ClickContinue();
        Console.WriteLine("✅ Submitted Add User form");
    }
}
