using CO.CDP.RegisterOfCommercialTools.App.Pages.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Auth;

public class LoginModelTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<ILogger<LoginModel>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly LoginModel _model;

    public LoginModelTests()
    {
        _mockSession = new Mock<ISession>();
        _mockLogger = new Mock<ILogger<LoginModel>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _model = new LoginModel(_mockSession.Object, _mockLogger.Object, _mockConfiguration.Object);

        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };
        _model.PageContext = pageContext;
    }

    [Fact]
    public void OnGet_ShouldReturnChallengeResult()
    {
        var result = _model.OnGet();

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be("/");
    }

    [Fact]
    public void OnGet_WithReturnUrl_ShouldSetRedirectUri()
    {
        var returnUrl = "/some/return/path";

        var result = _model.OnGet(returnUrl);

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be(returnUrl);
    }

    [Fact]
    public void OnGet_WithFtsOriginInAllowedList_ShouldSetFtsServiceOrigin()
    {
        var ftsOrigin = "https://find-tender-service.gov.uk";
        var allowedOrigins = "https://find-tender-service.gov.uk,https://other-service.gov.uk";

        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns(allowedOrigins);

        var result = _model.OnGet(origin: ftsOrigin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Remove(Session.FtsServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Remove(Session.SirsiServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, ftsOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnGet_WithSirsiOriginInAllowedList_ShouldSetSirsiServiceOrigin()
    {
        var sirsiOrigin = "https://supplier-registration.gov.uk";
        var allowedOrigins = "https://supplier-registration.gov.uk,https://other-service.gov.uk";

        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns("");
        _mockConfiguration.Setup(x => x["SirsiServiceAllowedOrigins"]).Returns(allowedOrigins);

        var result = _model.OnGet(origin: sirsiOrigin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Remove(Session.FtsServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Remove(Session.SirsiServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, sirsiOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnGet_WithOriginNotInAllowedList_ShouldNotSetServiceOrigin()
    {
        var unauthorizedOrigin = "https://malicious-site.com";
        var allowedOrigins = "https://find-tender-service.gov.uk,https://supplier-registration.gov.uk";

        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns(allowedOrigins);
        _mockConfiguration.Setup(x => x["SirsiServiceAllowedOrigins"]).Returns(allowedOrigins);

        var result = _model.OnGet(origin: unauthorizedOrigin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Remove(Session.FtsServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Remove(Session.SirsiServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, It.IsAny<string>()), Times.Never);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, It.IsAny<string>()), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(unauthorizedOrigin)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnGet_WithOriginInReturnUrl_ShouldExtractAndValidateOrigin()
    {
        var ftsOrigin = "https://find-tender-service.gov.uk";
        var returnUrl = $"/some/path?origin={Uri.EscapeDataString(ftsOrigin)}&other=param";
        var allowedOrigins = "https://find-tender-service.gov.uk";

        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns(allowedOrigins);

        var result = _model.OnGet(returnUrl);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, ftsOrigin), Times.Once);
    }

    [Fact]
    public void OnGet_WithMalformedReturnUrl_ShouldContinueExecution()
    {
        var malformedReturnUrl = "/%invalid%url%format%";

        var result = _model.OnGet(malformedReturnUrl);

        result.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public void OnGet_WithEmptyAllowedOrigins_ShouldNotSetServiceOrigins()
    {
        var origin = "https://some-service.gov.uk";
        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns("");
        _mockConfiguration.Setup(x => x["SirsiServiceAllowedOrigins"]).Returns("");

        var result = _model.OnGet(origin: origin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, It.IsAny<string>()), Times.Never);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnGet_WithNullConfiguration_ShouldNotSetServiceOrigins()
    {
        var origin = "https://some-service.gov.uk";
        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x["SirsiServiceAllowedOrigins"]).Returns((string?)null);

        var result = _model.OnGet(origin: origin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, It.IsAny<string>()), Times.Never);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnGet_ShouldAlwaysClearExistingServiceOrigins()
    {
        var result = _model.OnGet();

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Remove(Session.FtsServiceOrigin), Times.Once);
        _mockSession.Verify(x => x.Remove(Session.SirsiServiceOrigin), Times.Once);
    }

    [Fact]
    public void OnGet_WithBothOriginAndReturnUrlOrigin_ShouldPrioritizeDirectOriginParameter()
    {
        var directOrigin = "https://find-tender-service.gov.uk";
        var returnUrlOrigin = "https://supplier-registration.gov.uk";
        var returnUrl = $"/path?origin={Uri.EscapeDataString(returnUrlOrigin)}";
        var allowedOrigins = "https://find-tender-service.gov.uk,https://supplier-registration.gov.uk";

        _mockConfiguration.Setup(x => x["FtsServiceAllowedOrigins"]).Returns(allowedOrigins);

        var result = _model.OnGet(returnUrl, directOrigin);

        result.Should().BeOfType<ChallengeResult>();
        _mockSession.Verify(x => x.Set(Session.FtsServiceOrigin, directOrigin), Times.Once);
        _mockSession.Verify(x => x.Set(Session.SirsiServiceOrigin, It.IsAny<string>()), Times.Never);
    }
}