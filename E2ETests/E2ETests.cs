using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace E2ETests
{
    public class Tests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        [SetUp]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // Set to true if running in CI/CD
            });

            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
        }

        [Test]
        public async Task VerifyHeadingExists()
        {
            // Navigate to the webpage
            await _page.GotoAsync("http://localhost:8090/");

            // Wait for the heading to be visible (ensures the page has loaded)
            await _page.WaitForSelectorAsync("h1.govuk-heading-l");

            // Locate the heading and get its text content
            var headingText = await _page.InnerTextAsync("h1.govuk-heading-l");

            // Assert that the heading contains the expected text
            Assert.That(headingText, Does.Contain("Register your details as a buyer or supplier of procurements"));
        }

        [TearDown]
        public async Task TearDown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}
