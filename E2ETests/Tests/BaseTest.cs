using E2ETests.Pages;
using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests;

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

    protected static string SuperAdminEmail => ConfigUtility.GetTestSupportAdminEmail();
    private readonly string _superAdminPassword = ConfigUtility.GetTestSupportAdminPassword();
    private readonly string _superAdminSecretKey = ConfigUtility.GetTestSupportAdminSecretKey();

    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = ConfigUtility.IsHeadless()
        });

        _context = await _browser.NewContextAsync(new()
        {
            TracesDir = "Traces" // where to save traces
        });

        await _context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        _page = await _context.NewPageAsync();

        await Login();
    }

    protected async Task Login(bool isSuperAdminUser = false)
    {
        _loginPage = new LoginPage(_page);

        // Perform login before each test
        await _loginPage.Login(
            _baseUrl,
            isSuperAdminUser ? SuperAdminEmail : _testEmail,
            isSuperAdminUser ? _superAdminPassword : _testPassword,
            isSuperAdminUser ? _superAdminSecretKey : _secretKey);

        // Extract the access token ONCE
        if (_accessToken == null)
        {
            _accessToken = await AuthUtility.GetAccessToken(_page);
            Console.WriteLine($"🔑 Stored Access Token: {_accessToken}");
        }
    }

    protected async Task Logout()
    {
        await _page.ClickAsync("a.govuk-header__link[href='/one-login/sign-out']");
    }

    [TearDown]
    public async Task TearDown()
    {
        // Take screenshot if the test failed
        var result = TestContext.CurrentContext.Result.Outcome.Status;
        if (result == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotsDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            Directory.CreateDirectory(screenshotsDir);

            var filePath = Path.Combine(screenshotsDir, $"{TestContext.CurrentContext.Test.Name}.png");

            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = filePath,
                FullPage = true
            });

            TestContext.AddTestAttachment(filePath, "Screenshot on failure");
            Console.WriteLine($"📸 Screenshot saved: {filePath}");
        }

        // Add traces if the test failed
        if (result == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var tracePath = Path.Combine("Traces", $"{TestContext.CurrentContext.Test.Name}.zip");
            Directory.CreateDirectory("Traces");

            await _context.Tracing.StopAsync(new() { Path = tracePath });
            TestContext.AddTestAttachment(tracePath, "Playwright trace");
        }
        else
        {
            await _context.Tracing.StopAsync();
        }

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
