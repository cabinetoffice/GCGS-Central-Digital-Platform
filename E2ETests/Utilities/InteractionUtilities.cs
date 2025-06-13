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

        public async Task ClickSecondLinkByText(string linkText)
        {
            try
            {
                var links = page.Locator($"a:has-text(\"{linkText}\")");
                int count = await links.CountAsync();

                if (count < 2)
                {
                    Assert.Fail($"Expected at least 2 links with text '{linkText}', but found {count}.");
                }

                await links.Nth(1).ClickAsync();
                Console.WriteLine($"✅ Clicked second link with text: '{linkText}'");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to click second link with text '{linkText}'. Error: {ex.Message}");
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

        public Task UrlShouldBe(string expectedUrl)
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

            return Task.CompletedTask;
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

        public async Task ClickRadioButtonByText(string labelText)
        {
            try
            {
                var radioButton = page.GetByLabel(labelText);

                await radioButton.ClickAsync();
                Console.WriteLine($"✅ Clicked radio button with label: '{labelText}'");
            }
            catch (PlaywrightException pe)
            {
                string errorMessage = $"Playwright error while clicking radio button with label '{labelText}'. Error: {pe.Message}";
                if (pe.Message.Contains("Timeout"))
                {
                    errorMessage = $"Timeout while trying to click radio button with label '{labelText}'. Element might not be visible, enabled, or found within the timeout period. Error: {pe.Message}";
                }
                else if (pe.Message.ToLower().Contains("no element found for label") || pe.Message.ToLower().Contains("failed to find element matching label"))
                {
                    errorMessage = $"No radio button control found associated with the label '{labelText}'. Ensure the label text is exact and the label is correctly associated with a radio input. Error: {pe.Message}";
                }
                Assert.Fail(errorMessage);
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to click radio button with label '{labelText}'. Error: {ex.Message}");
            }
        }

        public async Task UploadFile()
        {
            try
            {
                string baseDirectory = System.AppContext.BaseDirectory;
                string projectRootPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDirectory, "..", "..", ".."));
                string resourceFilePath = System.IO.Path.Combine(projectRootPath, "Resources", "cat.jpeg");

                var fileInput = page.Locator("input[type='file']");
                if (await fileInput.CountAsync() == 0)
                {
                    Assert.Fail("No file input found on the page.");
                }

                await fileInput.SetInputFilesAsync(resourceFilePath);
                Console.WriteLine($"✅ Uploaded file: {resourceFilePath}");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to upload the predefined file (Resources/cat.jpeg). Error: {ex.Message}");
            }
        }

        public async Task PageShouldContainText(string expectedText)
        {
            try
            {
                var content = await page.ContentAsync();
                Assert.That(content, Does.Contain(expectedText), $"Page content was expected to contain '{expectedText}' but it did not.");
                Console.WriteLine($"✅ Page contains expected text: '{expectedText}'");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Failed to validate that page contains text '{expectedText}'. Error: {ex.Message}");
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