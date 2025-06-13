using Microsoft.Playwright;

namespace E2ETests.Utilities
{
    public class InteractionUtilities(IPage page)
    {
        public async Task ClickLinkByText(string linkText)
        {
            try
            {
                var link = page.Locator($"a:has-text(\"{linkText}\")");

                if (await link.First.IsVisibleAsync())
                {
                    await link.First.ClickAsync();
                }
                else
                {
                    var roleLink = page.GetByRole(AriaRole.Link, new() { Name = linkText });
                    if (await roleLink.IsVisibleAsync())
                    {
                        await roleLink.ClickAsync();
                    }
                    else
                    {
                        Assert.Fail(
                            $"Link with text '{linkText}' not found or not visible using 'a:has-text' or GetByRole.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to click link with text '{linkText}'. Error: {ex.Message}");
            }
        }

        public async Task ClickButtonByText(string buttonText)
        {
            try
            {
                var button = page.GetByRole(AriaRole.Button, new() { Name = buttonText });
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                }
                else
                {
                    Assert.Fail($"Button with text '{buttonText}' not found or not visible.");
                }
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to click button with text '{buttonText}'. Error: {ex.Message}");
            }
        }

        public async Task NavigateToUrl(string url)
        {
            try
            {
                await page.GotoAsync(url);
                Console.WriteLine($"📌 Navigated to URL: {url}");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to navigate to URL '{url}'. Error: {ex.Message}");
            }
        }

        public async Task PageTitleShouldBe(string expectedTitle)
        {
            try
            {
                var actualTitle = await page.TitleAsync();
                Assert.That(actualTitle, Is.EqualTo(expectedTitle), $"Page title was expected to be '{expectedTitle}' but was '{actualTitle}'.");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to validate page title. Expected: '{expectedTitle}'. Error: {ex.Message}");
            }
        }

        public async Task UrlShouldBe(string expectedUrl)
        {
            try
            {
                var actualUrl = page.Url;
                Assert.That(actualUrl, Is.EqualTo(expectedUrl), $"URL was expected to be '{expectedUrl}' but was '{actualUrl}'.");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to validate URL. Expected: '{expectedUrl}'. Error: {ex.Message}");
            }
        }

        public async Task EnterTextIntoTextArea(string text)
        {
            try
            {
                var textareas = page.Locator("textarea");
                int count = await textareas.CountAsync();

                if (count == 0)
                {
                    Assert.Fail("No textarea element found on the page. This method expects a unique textarea.");
                }
                if (count > 1)
                {
                    Assert.Fail($"Expected a unique textarea, but found {count} textarea elements. Please ensure only one textarea exists or use a more specific selector with another method.");
                }

                var field = textareas.First;

                await field.FillAsync(text, new LocatorFillOptions { Timeout = 10000 });

                Console.WriteLine($"✅ Entered text '{text}' into the unique textarea field.");
            }
            catch (PlaywrightException pe)
            {
                string errorMessage = $"Playwright error while entering text '{text}' into the unique textarea. Error: {pe.Message}";
                if (pe.Message.Contains("Timeout"))
                {
                    errorMessage = $"Timeout while trying to enter text '{text}' into the unique textarea. Element might not be visible, enabled, or found within the timeout period. Error: {pe.Message}";
                }
                else if (pe.Message.ToLower().Contains("element is not visible") || pe.Message.ToLower().Contains("element is hidden"))
                {
                     errorMessage = $"Textarea found but it is not visible. Cannot enter text '{text}'. Error: {pe.Message}";
                }
                Assert.Fail(errorMessage);
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to enter text '{text}' into the unique textarea. Error: {ex.Message}");
            }
        }

        public async Task WaitForPageLoad()
        {
            try
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                Console.WriteLine("✅ Page loaded successfully.");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to wait for page load. Error: {ex.Message}");
            }
        }
    }
}