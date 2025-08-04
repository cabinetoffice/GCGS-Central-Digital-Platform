using Microsoft.AspNetCore.Html;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Helpers;

public static class ButtonHelper
{
    public static IHtmlContent CreateFormButton(string? buttonText, PrimaryButtonStyle? buttonStyle = null, bool preventDoubleClick = true)
    {
        var isStartButton = buttonStyle == PrimaryButtonStyle.Start;
        var resolvedButtonText = ResolveButtonText(buttonText);

        var classes = isStartButton ? "govuk-button govuk-button--start" : "govuk-button";

        var builder = new HtmlContentBuilder()
            .AppendHtml($"<button type=\"submit\" class=\"{classes}\" data-module=\"govuk-button\"");

        if (preventDoubleClick && !isStartButton)
        {
            builder.AppendHtml(" data-prevent-double-click=\"true\"");
        }

        builder.AppendHtml($">{resolvedButtonText}");

        if (isStartButton)
        {
            builder.AppendHtml("<svg class=\"govuk-button__start-icon\" xmlns=\"http://www.w3.org/2000/svg\" width=\"17.5\" height=\"19\" viewBox=\"0 0 33 40\" aria-hidden=\"true\" focusable=\"false\">");
            builder.AppendHtml("<path fill=\"currentColor\" d=\"M0 0h13l20 20-20 20H0l20-20z\" />");
            builder.AppendHtml("</svg>");
        }

        builder.AppendHtml("</button>");

        return builder;
    }

    private static string ResolveButtonText(string? buttonText)
    {
        return string.IsNullOrWhiteSpace(buttonText) ? StaticTextResource.Global_Continue : buttonText;
    }
}