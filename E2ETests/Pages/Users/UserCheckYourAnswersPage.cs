using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class UserCheckYourAnswersPage
    {
        private readonly IPage _page;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string ChangeLinkSelector = "a:has-text('Change')";
        private readonly string SendEmailButtonSelector = "button:has-text('Send email')";
        private readonly string BackLinkSelector = "a.govuk-back-link";

        public UserCheckYourAnswersPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageTitle()
        {
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task ClickChange()
        {
            await _page.ClickAsync(ChangeLinkSelector);
            Console.WriteLine("🔁 Clicked 'Change' link to edit user details.");
        }

        public async Task ClickSendEmail()
        {
            await _page.ClickAsync(SendEmailButtonSelector);
            Console.WriteLine("📧 Clicked 'Send email' to invite the user.");
        }

        public async Task ClickBack()
        {
            await _page.ClickAsync(BackLinkSelector);
            Console.WriteLine("🔙 Clicked 'Back' to return to previous page.");
        }

        public async Task CompletePage()
        {
            await ClickSendEmail();
            Console.WriteLine("✅ Completed Check Your Answers by sending the invite.");
        }
    }
}