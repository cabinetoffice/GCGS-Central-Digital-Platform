using E2ETests.Pages;
using E2ETests.Utilities;
using Microsoft.Playwright;
using System.Threading;

namespace E2ETests;

public class BaseTest
{
    protected IBrowserContext _context;
    protected IPage _page;
    protected LoginPage _loginPage;
    protected static string? _accessToken; // Store the token globally

    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

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
    await PlaywrightHost.EnsureAsync();                 // shared browser
    _context = await PlaywrightHost.Browser!.NewContextAsync(); // isolated per test
    _page = await _context.NewPageAsync();

    // Start Playwright tracing for this test (screenshots & snapshots help debugging)
    await _context.Tracing.StartAsync(new TracingStartOptions
    {
        Screenshots = true,
        Snapshots = true,
        Sources = true
    });

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

        // Extract the access token ONCE (thread-safe for parallel runs)
        if (_accessToken == null)
        {
            await _tokenLock.WaitAsync();
            try
            {
                if (_accessToken == null) // double-check inside the lock
                {
                    _accessToken = await AuthUtility.GetAccessToken(_page);
                    Console.WriteLine($"🔑 Stored Access Token: {_accessToken}");
                }
            }
            finally
            {
                _tokenLock.Release();
            }
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
            // Save screenshot
            var screenshotsDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            Directory.CreateDirectory(screenshotsDir);

            var screenshotPath = Path.Combine(screenshotsDir, $"{TestContext.CurrentContext.Test.Name}.png");
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });
            TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");

            // Save trace
            var tracesDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Traces");
            Directory.CreateDirectory(tracesDir);

            var tracePath = Path.Combine(tracesDir, $"{TestContext.CurrentContext.Test.Name}.zip");
            await _context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = tracePath
            });
            TestContext.AddTestAttachment(tracePath, "Playwright trace");
        }
        else
        {
            // Stop without saving
            await _context.Tracing.StopAsync();
        }

            await _context.CloseAsync(); // close only the context
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
