using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class LoginPage
    {
        private readonly IPage _page;

        private readonly string SignInToAccountLink = "a.govuk-link[href='/one-login/sign-in']";
        private readonly string OneLoginSignInButton = "#sign-in-button";
        private readonly string OneLoginEmailAddressInputBox = "input.govuk-input#email";
        private readonly string OneLoginPasswordTextBox = "input.govuk-input#password";
        private readonly string AgreePrivacyNoticeCheckBox = "input.govuk-checkboxes__input#AgreeToPrivacy";
        private readonly string ContinueButton = "button.govuk-button[type='submit']";
        private readonly string EnterYourNameFirstNameTextBox = "input.govuk-input#FirstName";
        private readonly string EnterYourNameLastNameTextBox = "input.govuk-input#LastName";
        private readonly string MyAccountHeading = "h1.govuk-heading-l";
        private readonly string OtpInputBox = "input.govuk-input.govuk-input--width-10#code";

        public LoginPage(IPage page)
        {
            _page = page;
        }

        /// <summary>
        /// Performs login flow including handling OTP and extra steps if necessary.
        /// </summary>
        public async Task Login(string loginUrl, string email, string password, string secretKey)
        {
            await _page.GotoAsync(loginUrl);
            await _page.ClickAsync(SignInToAccountLink);
            await _page.ClickAsync(OneLoginSignInButton);
            await _page.FillAsync(OneLoginEmailAddressInputBox, email);
            await _page.ClickAsync(ContinueButton);
            await _page.FillAsync(OneLoginPasswordTextBox, password);
            await _page.ClickAsync(ContinueButton);

            // Wait for OTP input and enter generated OTP
            await _page.WaitForSelectorAsync(OtpInputBox);
            string otpCode = TOTPUtility.GenerateTOTP(secretKey);
            await _page.FillAsync(OtpInputBox, otpCode);
            await _page.ClickAsync(ContinueButton);

            // Check if "My Account" heading exists, if not, complete additional steps
            bool isMyAccountVisible = await _page.Locator(MyAccountHeading).IsVisibleAsync();

            if (!isMyAccountVisible)
            {
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
}
