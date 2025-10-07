using Microsoft.Playwright;
using System.Threading;

namespace E2ETests.Utilities;

public static class PlaywrightHost
{
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    public static IPlaywright? Playwright { get; private set; }
    public static IBrowser? Browser { get; private set; }

    public static async Task EnsureAsync()
    {
        if (Browser != null) return;
        await _initLock.WaitAsync();
        try
        {
            if (Browser == null)
            {
                Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                Browser = await Playwright.Chromium.LaunchAsync(new()
                {
                    Headless = ConfigUtility.IsHeadless(),   // true for parallel
                    Args = new[] { "--disable-dev-shm-usage" } // helps in Docker
                });
            }
        }
        finally { _initLock.Release(); }
    }
}