using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class OrganisationEmailAddressPage
    {
        private readonly IPage _page;

        // ✅ Selectors
        private const string PageHeadingSelector = "h1.govuk-heading-l";
        private const string EmailInputSelector = "input#EmailAddress";
        private const string SaveButtonSelector = "main >> button[type='submit']";

        public OrganisationEmailAddressPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageHeading()
        {
            await _page.WaitForSelectorAsync(PageHeadingSelector);
            return await _page.InnerTextAsync(PageHeadingSelector);
        }

        public async Task EnterEmailAddress(string email)
        {
            await _page.FillAsync(EmailInputSelector, email);
            Console.WriteLine($"✅ Entered email address: {email}");
        }

        public async Task ClickSave()
        {
            await _page.ClickAsync(SaveButtonSelector);
            Console.WriteLine("✅ Clicked 'Save' on Organisation Email Address Page");
        }

        public async Task CompletePage(string email)
        {
            await EnterEmailAddress(email);
            await ClickSave();
            Console.WriteLine("✅ Completed Organisation Email Address Page");
        }
    }
}