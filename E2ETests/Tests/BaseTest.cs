using System.Collections.Concurrent;
using System.Threading;
using E2ETests.Pages;
using E2ETests.Utilities;
using Microsoft.Playwright;
using NUnit.Framework;

namespace E2ETests;

public class BaseTest
{
    // ---------- per-fixture storage states & token ----------
    private static readonly ConcurrentDictionary<string, string> _fixtureStatePath = new();
    protected static string? _accessToken;            // shared token once fetched
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    // ---------- per-test context/page ----------
    protected IBrowserContext _context = default!;
    protected IPage _page = default!;
    protected LoginPage _loginPage = default!;

    // ---------- config ----------
    protected readonly string _baseUrl = ConfigUtility.GetBaseUrl();
    private readonly string _testEmail    = ConfigUtility.GetTestEmail();
    private readonly string _testPassword = ConfigUtility.GetTestPassword();
    private readonly string _secretKey    = ConfigUtility.GetSecretKey();
    protected static string SuperAdminEmail => ConfigUtility.GetTestSupportAdminEmail();
    private readonly string _superAdminPassword = ConfigUtility.GetTestSupportAdminPassword();
    private readonly string _superAdminSecretKey = ConfigUtility.GetTestSupportAdminSecretKey();

    // =========================================================
    // ONE-TIME LOGIN PER FIXTURE (saves a unique session cookie)
    // =========================================================
    [OneTimeSetUp]
    public async Task OneTimeLoginPerFixture()
    {
        await PlaywrightHost.EnsureAsync();

        var fixtureKey = GetType().FullName!; // unique per test class
        if (_fixtureStatePath.ContainsKey(fixtureKey)) return;

        var work = TestContext.CurrentContext.WorkDirectory;
        var dir  = Path.Combine(work, "AuthStates");
        Directory.CreateDirectory(dir);

        var statePath = Path.Combine(dir, fixtureKey.Replace('.', '_') + ".json");

        if (!File.Exists(statePath))
        {
            var tempCtx  = await PlaywrightHost.Browser!.NewContextAsync();
            var tempPage = await tempCtx.NewPageAsync();

            // Real OneLogin ONCE for this fixture
            _loginPage = new LoginPage(tempPage);
            await _loginPage.Login(_baseUrl, _testEmail, _testPassword, _secretKey);

            // (optional) do not rely on this being set; GetAccessToken() is lazy+locked
            await tempCtx.StorageStateAsync(new() { Path = statePath });
            await tempCtx.CloseAsync();

            Console.WriteLine($"✅ [{fixtureKey}] saved auth state: {statePath}");
        }

        _fixtureStatePath[fixtureKey] = statePath;
    }

    // ===========================================
    // PER-TEST: new isolated context from that state
    // ===========================================
    [SetUp]
    public async Task Setup()
    {
        await PlaywrightHost.EnsureAsync();

        var fixtureKey = GetType().FullName!;
        if (!_fixtureStatePath.TryGetValue(fixtureKey, out var statePath))
            throw new("Auth state not initialised. Did OneTimeSetUp run?");

        _context = await PlaywrightHost.Browser!.NewContextAsync(new() { StorageStatePath = statePath });
        _page    = await _context.NewPageAsync();

        // Tracing per test
        await _context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots   = true,
            Sources     = true
        });
    }

    // ===========================================
    // PER-TEST TEARDOWN: traces/screenshots & close
    // ===========================================
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
            TestContext.AddTestAttachment(shot, "Screenshot on failure");

            var traces = Path.Combine(work, "Traces");
            Directory.CreateDirectory(traces);
            var trace = Path.Combine(traces, $"{TestContext.CurrentContext.Test.Name}.zip");
            await _context.Tracing.StopAsync(new() { Path = trace });
            TestContext.AddTestAttachment(trace, "Playwright trace");
        }
        else
        {
            await _context.Tracing.StopAsync();
        }

        await _context.CloseAsync(); // only this test's context; fixture state remains
    }

    // =========================================================
    // HELPERS
    // =========================================================

    // Lazy, thread-safe token fetch — safe under parallel start-up
    protected string GetAccessToken()
    {
        if (_accessToken != null) return _accessToken;

        _tokenLock.Wait();
        try
        {
            if (_accessToken == null)
            {
                // Use a temporary page in the current authenticated context
                var tmpPage = _context.NewPageAsync().GetAwaiter().GetResult();
                try
                {
                    _accessToken = AuthUtility.GetAccessToken(tmpPage).GetAwaiter().GetResult();
                    Console.WriteLine($"🔑 Stored Access Token (lazy): {_accessToken}");
                }
                finally
                {
                    tmpPage.CloseAsync().GetAwaiter().GetResult();
                }
            }
        }
        finally
        {
            _tokenLock.Release();
        }

        return _accessToken!;
    }

    // Use only if you explicitly need to log in as a different identity mid-test.
    protected async Task Login(bool isSuperAdminUser = false)
    {
        _loginPage = new LoginPage(_page);
        await _loginPage.Login(
            _baseUrl,
            isSuperAdminUser ? SuperAdminEmail      : _testEmail,
            isSuperAdminUser ? _superAdminPassword  : _testPassword,
            isSuperAdminUser ? _superAdminSecretKey : _secretKey);
    }

    protected async Task Logout()
    {
        // Avoid calling this in setup/teardown for per-fixture mode; it will invalidate the server session.
        await _page.ClickAsync("a.govuk-header__link[href='/one-login/sign-out']");
    }

    public string GetBaseUrl() => _baseUrl;
}