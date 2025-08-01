using CO.CDP.OrganisationApp.Helpers;
using CO.CDP.OrganisationApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace CO.CDP.OrganisationApp.Tests.Helpers;

public class FormHeadingHelpersTests
{
    private static string GetHtmlContent(IHtmlContent htmlContent)
    {
        using var writer = new StringWriter();
        htmlContent.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
    [Theory]
    [InlineData(HeadingSize.ExtraLarge, true, "h1", "govuk-heading-xl")]
    [InlineData(HeadingSize.Large, true, "h1", "govuk-heading-l")]
    [InlineData(HeadingSize.Medium, true, "h2", "govuk-heading-m")]
    [InlineData(HeadingSize.Small, true, "h3", "govuk-heading-s")]
    [InlineData(null, true, "h1", "govuk-heading-l")]
    [InlineData(null, false, "h2", "govuk-heading-m")]
    public void RenderFieldsetHeading_ShouldRenderCorrectHtml(HeadingSize? headingSize, bool isFirst, string expectedTag, string expectedClass)
    {
        var heading = "Test Heading";
        var caption = "Test Caption";

        var result = FormHeadingHelpers.RenderFieldsetHeading(heading, caption, isFirst, headingSize);

        result.Should().NotBeNull();
        var htmlContent = GetHtmlContent(result);
        htmlContent.Should().Contain($"<{expectedTag} class=\"govuk-fieldset__heading {expectedClass}\">");
        htmlContent.Should().Contain($"<span class=\"govuk-caption-l govuk-!-margin-bottom-3\">{caption}</span>");
        htmlContent.Should().Contain(heading);
        htmlContent.Should().Contain($"</{expectedTag}>");
    }

    [Theory]
    [InlineData(HeadingSize.ExtraLarge, true, "h1", "xl")]
    [InlineData(HeadingSize.Large, true, "h1", "l")]
    [InlineData(HeadingSize.Medium, true, "h2", "m")]
    [InlineData(HeadingSize.Small, true, "h3", "s")]
    [InlineData(null, true, "h1", "l")]
    [InlineData(null, false, "h2", "m")]
    public void RenderLabelHeading_ShouldRenderCorrectHtml(HeadingSize? headingSize, bool isFirst, string expectedTag, string expectedLabelSize)
    {
        var heading = "Test Label Heading";
        var fieldId = "test-field-id";
        var caption = "Test Label Caption";

        var result = FormHeadingHelpers.RenderLabelHeading(heading, fieldId, caption, isFirst, headingSize);

        result.Should().NotBeNull();
        var htmlContent = GetHtmlContent(result);
        htmlContent.Should().Contain($"<{expectedTag} class=\"govuk-label-wrapper\">");
        htmlContent.Should().Contain($"<span class=\"govuk-caption-{expectedLabelSize}\">{caption}</span>");
        htmlContent.Should().Contain($"<label class=\"govuk-label govuk-label--{expectedLabelSize}\" for=\"{fieldId}\">{heading}</label>");
        htmlContent.Should().Contain($"</{expectedTag}>");
    }

    [Fact]
    public void RenderFieldsetHeading_ShouldReturnEmpty_WhenHeadingIsNull()
    {
        string? heading = null;

        var result = FormHeadingHelpers.RenderFieldsetHeading(heading);

        result.Should().Be(HtmlString.Empty);
    }

    [Fact]
    public void RenderFieldsetHeading_ShouldReturnEmpty_WhenHeadingIsEmpty()
    {
        string heading = "";

        var result = FormHeadingHelpers.RenderFieldsetHeading(heading);

        result.Should().Be(HtmlString.Empty);
    }

    [Fact]
    public void RenderLabelHeading_ShouldReturnEmpty_WhenHeadingIsNull()
    {
        string? heading = null;
        string fieldId = "some-id";

        var result = FormHeadingHelpers.RenderLabelHeading(heading, fieldId);

        result.Should().Be(HtmlString.Empty);
    }

    [Fact]
    public void RenderLabelHeading_ShouldReturnEmpty_WhenHeadingIsEmpty()
    {
        string heading = "";
        string fieldId = "some-id";

        var result = FormHeadingHelpers.RenderLabelHeading(heading, fieldId);

        result.Should().Be(HtmlString.Empty);
    }

    [Theory]
    [InlineData(HeadingSize.ExtraLarge, true, "h1", "govuk-heading-xl", "xl")]
    [InlineData(HeadingSize.Large, true, "h1", "govuk-heading-l", "l")]
    [InlineData(HeadingSize.Medium, true, "h2", "govuk-heading-m", "m")]
    [InlineData(HeadingSize.Small, true, "h3", "govuk-heading-s", "s")]
    [InlineData(null, true, "h1", "govuk-heading-l", "l")]
    [InlineData(null, false, "h2", "govuk-heading-m", "m")]
    public void RenderFieldsetLegend_ShouldRenderCorrectHtml(HeadingSize? headingSize, bool isFirst, string expectedTag, string expectedClass, string expectedLegendSize)
    {
        var heading = "Test Legend Heading";
        var caption = "Test Legend Caption";

        var result = FormHeadingHelpers.RenderLegend(heading, caption, isFirst, headingSize);

        result.Should().NotBeNull();
        var htmlContent = GetHtmlContent(result);
        htmlContent.Should().StartWith($"<legend class=\"govuk-fieldset__legend govuk-fieldset__legend--{expectedLegendSize}\">");
        htmlContent.Should().Contain($"<{expectedTag} class=\"{expectedClass}\">");
        htmlContent.Should().Contain($"<span class=\"govuk-caption-{expectedLegendSize}\">{caption}</span>");
        htmlContent.Should().Contain($"<{expectedTag} class=\"govuk-fieldset__heading\">{heading}</{expectedTag}>");
        htmlContent.Should().EndWith("</legend>");
    }

    [Fact]
    public void RenderFieldsetLegend_ShouldRenderWithoutCaption_WhenCaptionIsNull()
    {
        var heading = "Test Legend Heading";
        string? caption = null;

        var result = FormHeadingHelpers.RenderLegend(heading, caption);

        result.Should().NotBeNull();
        var htmlContent = GetHtmlContent(result);
        htmlContent.Should().StartWith("<legend class=\"govuk-fieldset__legend govuk-fieldset__legend--m\">");
        htmlContent.Should().Contain($"<h2 class=\"govuk-fieldset__heading\">{heading}</h2>");
        htmlContent.Should().NotContain("govuk-caption-l");
        htmlContent.Should().EndWith("</legend>");
    }

    [Fact]
    public void RenderFieldsetLegend_ShouldReturnEmpty_WhenHeadingIsNull()
    {
        string? heading = null;

        var result = FormHeadingHelpers.RenderLegend(heading);

        result.Should().Be(HtmlString.Empty);
    }

    [Fact]
    public void RenderFieldsetLegend_ShouldReturnEmpty_WhenHeadingIsEmpty()
    {
        string heading = "";

        var result = FormHeadingHelpers.RenderLegend(heading);

        result.Should().Be(HtmlString.Empty);
    }

}
