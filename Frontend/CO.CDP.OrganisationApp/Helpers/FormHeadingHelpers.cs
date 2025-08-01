using Microsoft.AspNetCore.Html;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Helpers;

public static class FormHeadingHelpers
{
    private static string GetGovUkSizeSuffix(HeadingSize? headingSize, bool isFirst)
    {
        return headingSize switch
        {
            HeadingSize.ExtraLarge => "xl",
            HeadingSize.Large => "l",
            HeadingSize.Medium => "m",
            HeadingSize.Small => "s",
            _ => isFirst ? "l" : "m"
        };
    }

    private static (string tag, string cssClass) GetHeadingTagAndClass(HeadingSize? headingSize, bool isFirst)
    {
        return headingSize switch
        {
            HeadingSize.ExtraLarge => ("h1", "govuk-heading-xl"),
            HeadingSize.Large => ("h1", "govuk-heading-l"),
            HeadingSize.Medium => ("h2", "govuk-heading-m"),
            HeadingSize.Small => ("h3", "govuk-heading-s"),
            _ => (isFirst ? "h1" : "h2", isFirst ? "govuk-heading-l" : "govuk-heading-m")
        };
    }

    public static IHtmlContent RenderFieldsetHeading(string? heading, string? caption = null, bool isFirst = false,
        HeadingSize? headingSize = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var (headingTag, headingClass) = GetHeadingTagAndClass(headingSize, isFirst);
        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        content.AppendHtml($"<{headingTag} class=\"govuk-fieldset__heading {headingClass}\">");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.AppendHtml(
                $"<span class=\"govuk-caption-{govUkSizeSuffix} govuk-!-margin-bottom-3\">{caption}</span>");
        }

        content.AppendHtml(heading);
        content.AppendHtml($"</{headingTag}>");

        return content;
    }

    public static IHtmlContent RenderLabelHeading(string? heading, string fieldId, string? caption = null,
        bool isFirst = false, HeadingSize? headingSize = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var (headingTag, headingClass) = GetHeadingTagAndClass(headingSize, isFirst);
        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        content.AppendHtml($"<{headingTag} class=\"govuk-label-wrapper\">");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.AppendHtml($"<span class=\"govuk-caption-{govUkSizeSuffix}\">{caption}</span>");
        }

        content.AppendHtml(
            $"<label class=\"govuk-label govuk-label--{govUkSizeSuffix}\" for=\"{fieldId}\">{heading}</label>");
        content.AppendHtml($"</{headingTag}>");

        return content;
    }

    public static IHtmlContent RenderLabelHeadingWithCaptionAfter(string? heading, string fieldId, string? caption = null,
        bool isFirst = false, HeadingSize? headingSize = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        content.AppendHtml("<h1 class=\"govuk-label-wrapper\">");
        content.AppendHtml(
            $"<label class=\"govuk-label govuk-label--{govUkSizeSuffix}\" for=\"{fieldId}\">{heading}</label>");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.AppendHtml($"<span class=\"govuk-caption-{govUkSizeSuffix} govuk-!-margin-bottom-3\">{caption}</span>");
        }

        content.AppendHtml("</h1>");

        return content;
    }

    public static IHtmlContent RenderLegend(string? heading, string? caption = null,
        bool isFirst = false, HeadingSize? headingSize = null, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);

        var (headingTag, _) = GetHeadingTagAndClass(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        var idAttribute = string.IsNullOrWhiteSpace(id) ? "" : $" id=\"{id}\"";
        content.AppendHtml(
            $"<legend class=\"govuk-fieldset__legend govuk-fieldset__legend--{govUkSizeSuffix}\"{idAttribute}>");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.AppendHtml($"<span class=\"govuk-caption-{govUkSizeSuffix}\">{caption}</span>");
        }

        content.AppendHtml($"<{headingTag} class=\"govuk-fieldset__heading\">{heading}</{headingTag}>");
        content.AppendHtml("</legend>");

        return content;
    }

    public static IHtmlContent RenderLegendWithoutCaption(string? heading, bool isFirst = false, HeadingSize? headingSize = null, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);
        var (headingTag, _) = GetHeadingTagAndClass(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        var idAttribute = string.IsNullOrWhiteSpace(id) ? "" : $" id=\"{id}\"";
        content.AppendHtml(
            $"<legend class=\"govuk-fieldset__legend govuk-fieldset__legend--{govUkSizeSuffix}\"{idAttribute}>");
        content.AppendHtml($"<{headingTag} class=\"govuk-fieldset__heading\">{heading}</{headingTag}>");
        content.AppendHtml("</legend>");

        return content;
    }

    public static IHtmlContent RenderHeading(string? heading, string? caption = null, bool isFirst = false,
        HeadingSize? headingSize = null)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return HtmlString.Empty;

        var govUkSizeSuffix = GetGovUkSizeSuffix(headingSize, isFirst);

        var (headingTag, headingClass) = GetHeadingTagAndClass(headingSize, isFirst);

        var content = new HtmlContentBuilder();
        content.AppendHtml($"<{headingTag} class=\"{headingClass}\">");

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.AppendHtml($"<span class=\"govuk-caption-{govUkSizeSuffix}\">{caption}</span>");
        }

        content.AppendHtml(heading);
        content.AppendHtml($"</{headingTag}>");

        return content;
    }
}