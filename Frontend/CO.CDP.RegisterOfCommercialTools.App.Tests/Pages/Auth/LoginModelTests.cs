using CO.CDP.RegisterOfCommercialTools.App.Pages.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Auth;

public class LoginModelTests
{
    private readonly LoginModel _model = new();

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
}