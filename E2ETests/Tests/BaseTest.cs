using E2ETests.Pages;
using E2ETests.Utilities;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading;

namespace E2ETests;

public class BaseTest
{
    protected IBrowserContext _context = default!;
    protected IPage _page = default!;
    protected LoginPage _loginPage = default!;

    // Static so it’s shared across the entire run
    private static string? _authStatePath;
    private static string? _accessToken;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    protected readonly string _baseUrl = ConfigUtility.GetBaseUrl();
    private readonly string _testEmail = ConfigUtility.GetTestEmail();
    private readonly string _testPassword = ConfigUtility.GetTestPassword();
    private readonly string _secretKey = ConfigUtility.GetSecretKey();

    protected static string SuperAdminEmail => ConfigUtility.GetTestSupportAdminEmail();
    private readonly string _superAdminPassword = ConfigUtility.GetTestSupportAdminPassword();
    private readonly string _superAdminSecretKey = ConfigUtility.GetTestSupportAdminSecretKey();

    // ---------- One-time login ----------
    [OneTimeSetUp]
    public async Task GlobalLogin()
    {
        await PlaywrightHost.EnsureAsync();

        var work = TestContext.CurrentContext.WorkDirectory;
        var dir = Path.Combine(work, "AuthStates");
        Directory.CreateDirectory(dir);

        _authStatePath = Path.Combine(dir, "global.json");

        if (!File.Exists(_authStatePath))
        {
            var ctx = await PlaywrightHost.Browser!.NewContextAsync();
            var page = await ctx.NewPageAsync();

            // Do real login ONCE
            _loginPage = new LoginPage(page);
            await _loginPage.Login(_baseUrl, _testEmail, _testPassword, _secretKey);

            // Capture token once
            _accessToken = await AuthUtility.GetAccessToken(page);

            // Save storage state to disk
            await ctx.StorageStateAsync(new() { Path = _authStatePath });
            await ctx.CloseAsync();

            Console.WriteLine("✅ Logged in once and saved auth state.");
        }
    }

    // ---------- Per-test setup ----------
    [SetUp]
    public async Task Setup()
    {
        await PlaywrightHost.EnsureAsync();

        if (string.IsNullOrEmpty(_authStatePath))
            throw new("Auth state missing. Did GlobalLogin run?");

        // Create a new context with the saved signed-in state
        _context = await PlaywrightHost.Browser!.NewContextAsync(new() { StorageStatePath = _authStatePath });
        _page = await _context.NewPageAsync();

        await _context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    // ---------- Tear down ----------
    [TearDown]
    public async Task TearDown()
    {
        var result = TestContext.CurrentContext.Result.Outcome.Status;

        if (result == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var work = TestContext.CurrentContext.WorkDirectory;

            var shots = Path.Combine(work, "Screenshots");
            Directory.CreateDirectory(shots);
            var shot = Path.Combine(shots, $"{TestContext.CurrentContext.Test.Name}.png");
            await _page.ScreenshotAsync(new() { Path = shot, FullPage = true });
            TestContext.AddTestAttachment(shot, "Screenshot");

            var traces = Path.Combine(work, "Traces");
            Directory.CreateDirectory(traces);
            var trace = Path.Combine(traces, $"{TestContext.CurrentContext.Test.Name}.zip");
            await _context.Tracing.StopAsync(new() { Path = trace });
            TestContext.AddTestAttachment(trace, "Trace");
        }
        else
        {
            await _context.Tracing.StopAsync();
        }

        await _context.CloseAsync(); // safe: doesn’t affect the saved global state
    }

    // ---------- Helpers ----------
protected string GetAccessToken()
{
    if (_accessToken != null) return _accessToken;

    // Make sure we only fetch once across all workers
    _tokenLock.Wait();
    try
    {
        if (_accessToken == null)
        {
            // Use a fresh page in the current authenticated context so we don't
            // disturb whatever _page is doing in the test.
            var tokenPageTask = _context.NewPageAsync(); // _context is already authed from storage state
            tokenPageTask.Wait();
            var tokenPage = tokenPageTask.Result;

            try
            {
                // AuthUtility navigates to /diagnostic and returns the token
                var tokenTask = AuthUtility.GetAccessToken(tokenPage);
                tokenTask.Wait();
                _accessToken = tokenTask.Result;
                Console.WriteLine($"🔑 Stored Access Token (lazy): {_accessToken}");
            }
            finally
            {
                // close the temporary page
                tokenPage.CloseAsync().Wait();
            }
        }
    }
    finally
    {
        _tokenLock.Release();
    }

    return _accessToken!;
}
    public string GetBaseUrl() => _baseUrl;
}