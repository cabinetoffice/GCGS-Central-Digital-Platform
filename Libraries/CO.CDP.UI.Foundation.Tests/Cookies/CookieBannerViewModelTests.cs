using CO.CDP.UI.Foundation.Cookies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Cookies;

public class CookieBannerViewModelTests
{
    private readonly Mock<ICookiePreferencesService> _mockCookieService;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly CookieSettings _cookieSettings;

    public CookieBannerViewModelTests()
    {
        _mockCookieService = new Mock<ICookiePreferencesService>();
        _mockUrlHelper = new Mock<IUrlHelper>();
        _cookieSettings = new CookieSettings();

        _mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
    }

    private CookieBannerViewModel CreateViewModel(string path = "/", string pathBase = "", string? queryString = "") =>
        new(
            _mockCookieService.Object,
            _cookieSettings,
            _mockUrlHelper.Object,
            path,
            pathBase,
            queryString);

    #region ShouldShowConsentBanner Tests

    [Fact]
    public void ShouldShowConsentBanner_WhenUnknownAndNoInteractionAndNotCookiesPage_ReturnsTrue()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(true);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConsentBanner(hasInteractionQuery: false);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldShowConsentBanner_WhenUnknownButHasInteraction_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(true);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConsentBanner(hasInteractionQuery: true);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowConsentBanner_WhenOnCookiesPage_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(true);
        var viewModel = CreateViewModel("/cookies");

        var result = viewModel.ShouldShowConsentBanner(hasInteractionQuery: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowConsentBanner_WhenOnCookiesPageWithDifferentCase_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(true);
        var viewModel = CreateViewModel("/Cookies");

        var result = viewModel.ShouldShowConsentBanner(hasInteractionQuery: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowConsentBanner_WhenNotUnknown_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(false);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConsentBanner(hasInteractionQuery: false);

        result.Should().BeFalse();
    }

    #endregion

    #region ShouldShowConfirmationBanner Tests

    [Fact]
    public void ShouldShowConfirmationBanner_WhenNotUnknownAndHasInteraction_ReturnsTrue()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(false);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConfirmationBanner(hasInteractionQuery: true);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldShowConfirmationBanner_WhenNotUnknownButNoInteraction_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(false);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConfirmationBanner(hasInteractionQuery: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowConfirmationBanner_WhenUnknown_ReturnsFalse()
    {
        _mockCookieService.Setup(s => s.IsUnknown()).Returns(true);
        var viewModel = CreateViewModel("/some-page");

        var result = viewModel.ShouldShowConfirmationBanner(hasInteractionQuery: true);

        result.Should().BeFalse();
    }

    #endregion

    #region GetReturnUrl Tests

    [Fact]
    public void GetReturnUrl_RemovesFtsHandoverParameter()
    {
        var viewModel = CreateViewModel("/page", "",
            $"?{_cookieSettings.CookiesAcceptedHandoverParameter}=true&other=value");

        var result = viewModel.GetReturnUrl();

        result.Should().Be("/page?other=value");
        result.Should().NotContain(_cookieSettings.CookiesAcceptedHandoverParameter);
    }

    [Fact]
    public void GetReturnUrl_WhenOnlyFtsParameter_ReturnsPathWithoutQuery()
    {
        var viewModel = CreateViewModel("/page", "", $"?{_cookieSettings.CookiesAcceptedHandoverParameter}=true");

        var result = viewModel.GetReturnUrl();

        result.Should().Be("/page");
    }

    [Fact]
    public void GetReturnUrl_WhenNoQueryString_ReturnsPath()
    {
        var viewModel = CreateViewModel("/page");

        var result = viewModel.GetReturnUrl();

        result.Should().Be("/page");
    }

    [Fact]
    public void GetReturnUrl_WhenNoFtsParameter_PreservesOtherParameters()
    {
        var viewModel = CreateViewModel("/page", "", "?param1=value1&param2=value2");

        var result = viewModel.GetReturnUrl();

        result.Should().Be("/page?param1=value1&param2=value2");
    }

    [Fact]
    public void GetReturnUrl_IncludesPathBase()
    {
        var viewModel = CreateViewModel("/page", "/app", "?param=value");

        var result = viewModel.GetReturnUrl();

        result.Should().StartWith("/app");
        result.Should().Be("/app/page?param=value");
    }

    [Fact]
    public void GetReturnUrl_WhenNullQueryString_ReturnsPathWithoutError()
    {
        var viewModel = CreateViewModel("/page", "", null);

        var result = viewModel.GetReturnUrl();

        result.Should().Be("/page");
    }

    #endregion

    #region GetHideUrl Tests

    [Fact]
    public void GetHideUrl_RemovesCookieBannerInteractionParameter()
    {
        var viewModel = CreateViewModel("/page", "",
            $"?{_cookieSettings.CookieBannerInteractionQueryString}=true&other=value");

        var result = viewModel.GetHideUrl();

        result.Should().NotContain(_cookieSettings.CookieBannerInteractionQueryString);
        result.Should().Contain("other=value");
    }

    [Fact]
    public void GetHideUrl_WhenOnlyInteractionParameter_ReturnsPathWithoutQuery()
    {
        var viewModel = CreateViewModel("/page", "", $"?{_cookieSettings.CookieBannerInteractionQueryString}=true");

        var result = viewModel.GetHideUrl();

        result.Should().Be("/page");
    }

    [Fact]
    public void GetHideUrl_WhenQueryStringIsJustQuestionMark_ReturnsPath()
    {
        var viewModel = CreateViewModel("/page", "", "?");

        var result = viewModel.GetHideUrl();

        result.Should().Be("/page");
    }

    [Fact]
    public void GetHideUrl_IncludesPathBase()
    {
        var viewModel = CreateViewModel("/page", "/app", $"?{_cookieSettings.CookieBannerInteractionQueryString}=true");

        var result = viewModel.GetHideUrl();

        result.Should().StartWith("/app");
    }

    #endregion

    #region GetConfirmationMessage Tests

    [Fact]
    public void GetConfirmationMessage_WhenAccepted_ReturnsAcceptedStatement()
    {
        _mockCookieService.Setup(s => s.IsAccepted()).Returns(true);
        var viewModel = CreateViewModel("/page");

        var result = viewModel.GetConfirmationMessage();

        result.Should().Be(Localization.StaticTextResource.CookieBanner_AcceptedStatement);
    }

    [Fact]
    public void GetConfirmationMessage_WhenRejected_ReturnsRejectedStatement()
    {
        _mockCookieService.Setup(s => s.IsAccepted()).Returns(false);
        var viewModel = CreateViewModel("/page");

        var result = viewModel.GetConfirmationMessage();

        result.Should().Be(Localization.StaticTextResource.CookieBanner_RejectedStatement);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void CookieAcceptanceFieldName_ReturnsValueFromSettings()
    {
        var viewModel = CreateViewModel("/page");

        viewModel.CookieAcceptanceFieldName.Should().Be(_cookieSettings.CookieAcceptanceFieldName);
    }

    [Fact]
    public void ReturnUrlFieldName_ReturnsValueFromSettings()
    {
        var viewModel = CreateViewModel("/page");

        viewModel.ReturnUrlFieldName.Should().Be(_cookieSettings.CookieSettingsPageReturnUrlFieldName);
    }

    #endregion

    #region URL Validation Tests

    [Fact]
    public void Constructor_WhenPathIsNotLocal_SanitisesToRoot()
    {
        _mockUrlHelper.Setup(u => u.IsLocalUrl("//evil.com")).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/")).Returns(true);

        var viewModel = CreateViewModel("//evil.com");

        var result = viewModel.GetReturnUrl();
        result.Should().StartWith("/");
        result.Should().NotContain("evil.com");
    }

    [Fact]
    public void Constructor_WhenPathBaseIsNotLocal_SanitisesToRoot()
    {
        _mockUrlHelper.Setup(u => u.IsLocalUrl("http://evil.com")).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/")).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/page")).Returns(true);

        var viewModel = CreateViewModel("/page", "http://evil.com");

        var result = viewModel.GetReturnUrl();
        result.Should().StartWith("/");
        result.Should().NotContain("evil.com");
    }

    [Fact]
    public void Constructor_WhenPathIsNull_SanitisesToRoot()
    {
        _mockUrlHelper.Setup(u => u.IsLocalUrl(null!)).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/")).Returns(true);

        var viewModel = CreateViewModel(null!);

        var result = viewModel.GetReturnUrl();
        result.Should().Be("/");
    }

    [Fact]
    public void Constructor_WhenPathIsEmpty_SanitisesToRoot()
    {
        var viewModel = CreateViewModel("");

        var result = viewModel.GetReturnUrl();
        result.Should().Be("/");
    }

    [Fact]
    public void Constructor_WhenValidLocalPath_PreservesPath()
    {
        _mockUrlHelper.Setup(u => u.IsLocalUrl("/cookies")).Returns(true);

        var viewModel = CreateViewModel("/cookies");

        var result = viewModel.GetReturnUrl();
        result.Should().Contain("/cookies");
    }

    #endregion
}