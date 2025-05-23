using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class HowOrganisationRegisteredPage
    {
        private readonly IPage _page;

        // ✅ Selectors
        private const string PageHeadingSelector = "h1.govuk-fieldset__heading";
        private const string ContinueButtonSelector = "main >> button[type='submit']";
        private const string OtherInputSelector = "#OtherLegalForm";

        public HowOrganisationRegisteredPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageHeading()
        {
            await _page.WaitForSelectorAsync(PageHeadingSelector);
            return await _page.InnerTextAsync(PageHeadingSelector);
        }

        public async Task SelectRegistrationType(string value, string otherText = null)
        {
            var selector = $"input[name='RegisteredOrg'][value='{value}']";
            await _page.CheckAsync(selector);

            if (value == "Other" && !string.IsNullOrEmpty(otherText))
            {
                await _page.FillAsync(OtherInputSelector, otherText);
            }

            System.Console.WriteLine($"✅ Selected registration type: {value}" + (value == "Other" ? $" with detail: {otherText}" : ""));
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
            System.Console.WriteLine("✅ Clicked 'Continue' on How Organisation Registered Page");
        }

        public async Task CompletePage(string registrationType, string otherText = null)
        {
            await SelectRegistrationType(registrationType, otherText);
            await ClickContinue();
            System.Console.WriteLine("✅ Completed How Organisation Registered Page");
        }
    }
}