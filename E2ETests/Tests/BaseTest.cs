using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using E2ETests.Utilities;
using E2ETests.Pages;

namespace E2ETests
{
    public class BaseTest
    {
        protected IPlaywright _playwright;
        protected IBrowser _browser;
        protected IBrowserContext _context;
        protected IPage _page;
        protected LoginPage _loginPage;
        protected static string? _accessToken; // Store the token globally

        // Retrieve configuration values dynamically
        protected readonly string _baseUrl = ConfigUtility.GetBaseUrl();
        private readonly string _testEmail = ConfigUtility.GetTestEmail();
        private readonly string _testPassword = ConfigUtility.GetTestPassword();
        private readonly string _secretKey = ConfigUtility.GetSecretKey();

        [SetUp]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // Set to true for CI/CD
            });

            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
            _loginPage = new LoginPage(_page);

            // Perform login before each test
            await _loginPage.Login(_baseUrl, _testEmail, _testPassword, _secretKey);

            // Extract the access token ONCE
            if (_accessToken == null)
            {
                _accessToken = await AuthUtility.GetAccessToken(_page);
                Console.WriteLine($"🔑 Stored Access Token: {_accessToken}");
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        // Provide the stored access token for tests that need API requests
        protected string GetAccessToken()
        {
            if (_accessToken == null)
                throw new Exception("Access token not available. Ensure BaseTest has run.");
            return _accessToken;
        }

        // Provide the base URL for page objects
        public string GetBaseUrl()
        {
            return _baseUrl;
        }
    }
}
