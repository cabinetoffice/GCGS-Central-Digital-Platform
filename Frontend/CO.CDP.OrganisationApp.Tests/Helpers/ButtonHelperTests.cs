using CO.CDP.OrganisationApp.Helpers;
using CO.CDP.OrganisationApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Html;

namespace CO.CDP.OrganisationApp.Tests.Helpers;

public class ButtonHelperTests
{
    [Fact]
    public void CreateFormButton_WithDefaultParameters_ReturnsDefaultButton()
    {
        var result = ButtonHelper.CreateFormButton(null);
        var html = GetHtmlString(result);

        html.Should().Contain("type=\"submit\"");
        html.Should().Contain("class=\"govuk-button\"");
        html.Should().Contain("data-module=\"govuk-button\"");
        html.Should().Contain("data-prevent-double-click=\"true\"");
        html.Should().Contain("Continue");
        html.Should().NotContain("govuk-button--start");
        html.Should().NotContain("govuk-button__start-icon");
    }

    [Fact]
    public void CreateFormButton_WithCustomButtonText_UsesProvidedText()
    {
        var buttonText = "Save and continue";

        var result = ButtonHelper.CreateFormButton(buttonText);
        var html = GetHtmlString(result);

        html.Should().Contain($">{buttonText}<");
        html.Should().NotContain("Continue");
    }

    [Fact]
    public void CreateFormButton_WithEmptyButtonText_UsesDefaultContinueText()
    {
        var result = ButtonHelper.CreateFormButton("");
        var html = GetHtmlString(result);

        html.Should().Contain(">Continue<");
    }

    [Fact]
    public void CreateFormButton_WithWhitespaceButtonText_UsesDefaultContinueText()
    {
        var result = ButtonHelper.CreateFormButton("   ");
        var html = GetHtmlString(result);

        html.Should().Contain(">Continue<");
    }

    [Fact]
    public void CreateFormButton_WithStartButtonStyle_RendersStartButton()
    {
        var result = ButtonHelper.CreateFormButton("Start now", PrimaryButtonStyle.Start);
        var html = GetHtmlString(result);

        html.Should().Contain("class=\"govuk-button govuk-button--start\"");
        html.Should().Contain("data-module=\"govuk-button\"");
        html.Should().Contain(">Start now<");
        html.Should().Contain("govuk-button__start-icon");
        html.Should().Contain("<svg");
        html.Should().Contain("<path fill=\"currentColor\" d=\"M0 0h13l20 20-20 20H0l20-20z\" />");
        html.Should().Contain("data-prevent-double-click");
    }

    [Fact]
    public void CreateFormButton_WithDefaultButtonStyle_RendersDefaultButton()
    {
        var result = ButtonHelper.CreateFormButton("Continue", PrimaryButtonStyle.Default);
        var html = GetHtmlString(result);

        html.Should().Contain("class=\"govuk-button\"");
        html.Should().NotContain("govuk-button--start");
        html.Should().Contain("data-prevent-double-click=\"true\"");
        html.Should().NotContain("govuk-button__start-icon");
    }

    [Fact]
    public void CreateFormButton_WithPreventDoubleClickFalse_DoesNotIncludePreventDoubleClick()
    {
        var result = ButtonHelper.CreateFormButton("Continue", preventDoubleClick: false);
        var html = GetHtmlString(result);

        html.Should().NotContain("data-prevent-double-click");
    }

    [Fact]
    public void CreateFormButton_WithPreventDoubleClickTrue_IncludesPreventDoubleClick()
    {
        var result = ButtonHelper.CreateFormButton("Continue", preventDoubleClick: true);
        var html = GetHtmlString(result);

        html.Should().Contain("data-prevent-double-click=\"true\"");
    }

    [Fact]
    public void CreateFormButton_StartButtonWithPreventDoubleClickTrue_IncludesPreventDoubleClick()
    {
        var result = ButtonHelper.CreateFormButton("Start now", PrimaryButtonStyle.Start, preventDoubleClick: true);
        var html = GetHtmlString(result);

        html.Should().Contain("data-prevent-double-click=\"true\"");
    }

    [Fact]
    public void CreateFormButton_StartButtonWithPreventDoubleClickFalse_DoesNotIncludePreventDoubleClick()
    {
        var result = ButtonHelper.CreateFormButton("Start now", PrimaryButtonStyle.Start, preventDoubleClick: false);
        var html = GetHtmlString(result);

        html.Should().NotContain("data-prevent-double-click");
    }

    [Theory]
    [InlineData("Save")]
    [InlineData("Submit")]
    [InlineData("Next")]
    [InlineData("Finish")]
    public void CreateFormButton_WithVariousButtonTexts_RendersCorrectText(string buttonText)
    {
        var result = ButtonHelper.CreateFormButton(buttonText);
        var html = GetHtmlString(result);

        html.Should().Contain($">{buttonText}<");
    }

    [Fact]
    public void CreateFormButton_StartButtonHasCorrectSvgAttributes()
    {
        var result = ButtonHelper.CreateFormButton("Start", PrimaryButtonStyle.Start);
        var html = GetHtmlString(result);

        html.Should().Contain("xmlns=\"http://www.w3.org/2000/svg\"");
        html.Should().Contain("width=\"17.5\"");
        html.Should().Contain("height=\"19\"");
        html.Should().Contain("viewBox=\"0 0 33 40\"");
        html.Should().Contain("aria-hidden=\"true\"");
        html.Should().Contain("focusable=\"false\"");
    }

    [Fact]
    public void CreateFormButton_AlwaysIncludesRequiredAttributes()
    {
        var result = ButtonHelper.CreateFormButton("Test");
        var html = GetHtmlString(result);

        html.Should().StartWith("<button");
        html.Should().Contain("type=\"submit\"");
        html.Should().Contain("data-module=\"govuk-button\"");
        html.Should().EndWith("</button>");
    }

    private static string GetHtmlString(IHtmlContent htmlContent)
    {
        using var writer = new StringWriter();
        htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }
}