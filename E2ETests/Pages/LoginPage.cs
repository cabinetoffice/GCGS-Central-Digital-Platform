using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class LoginPage
{
    private readonly IPage _page;

    private readonly string SignInToAccountLink = "a.govuk-link[href='/one-login/sign-in']";
    private readonly string SignOutLink = "a.govuk-header__link[href='/one-login/sign-out']";
    private readonly string OneLoginSignInButton = "#sign-in-button";
    private readonly string OneLoginEmailAddressInputBox = "input.govuk-input#email";
    private readonly string OneLoginPasswordTextBox = "input.govuk-input#password";
    private readonly string AgreePrivacyNoticeCheckBox = "input.govuk-checkboxes__input#AgreeToPrivacy";
    private readonly string ContinueButton = "button.govuk-button[type='submit']";
    private readonly string EnterYourNameFirstNameTextBox = "input.govuk-input#FirstName";
    private readonly string EnterYourNameLastNameTextBox = "input.govuk-input#LastName";
    private readonly string OtpInputBox = "input.govuk-input.govuk-input--width-10#code";

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task Login(string loginUrl, string email, string password, string secretKey)
    {
        await _page.GotoAsync(loginUrl);
        await _page.ClickAsync(SignInToAccountLink);

        bool isSignOutVisible = await _page.Locator(SignOutLink).IsVisibleAsync();

        if (!isSignOutVisible) {

            var url = _page.Url;
            TestContext.Progress.WriteLine($"Page URL during login: {url}");

            Directory.CreateDirectory(Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "Screenshots"
            ));

            await _page.ScreenshotAsync(new()
            {
                Path = Path.Combine(
                    TestContext.CurrentContext.WorkDirectory,
                    "Screenshots",
                    $"{TestContext.CurrentContext.Test.Name}-login.png"
                ),
                FullPage = true
            });


            await _page.ClickAsync(OneLoginSignInButton);
            await _page.FillAsync(OneLoginEmailAddressInputBox, email);
            await _page.ClickAsync(ContinueButton);
            await _page.FillAsync(OneLoginPasswordTextBox, password);
            await _page.ClickAsync(ContinueButton);

            await _page.WaitForSelectorAsync(OtpInputBox);
            string otpCode = TOTPUtility.GenerateTOTP(secretKey);
            await _page.FillAsync(OtpInputBox, otpCode);
            await _page.ClickAsync(ContinueButton);
        }

        bool isPrivacyNoticeCheckboxVisible = await _page.Locator(AgreePrivacyNoticeCheckBox).IsVisibleAsync();

        if (isPrivacyNoticeCheckboxVisible)
        {
            await _page.ClickAsync(AgreePrivacyNoticeCheckBox);
            await _page.ClickAsync(ContinueButton);
            await _page.ClickAsync(AgreePrivacyNoticeCheckBox);
            await _page.ClickAsync(ContinueButton);
            await _page.WaitForSelectorAsync(EnterYourNameFirstNameTextBox);
            await _page.FillAsync(EnterYourNameFirstNameTextBox, "Test");
            await _page.ClickAsync(ContinueButton);
            await _page.FillAsync(EnterYourNameLastNameTextBox, "User");
            await _page.ClickAsync(ContinueButton);
        }
    }
}
