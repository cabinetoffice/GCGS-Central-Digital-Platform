using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using System.Resources;

namespace CO.CDP.OrganisationApp.Helpers;

public static class InputSuffixHelper
{
    public class SuffixRenderInfo
    {
        public bool HasSuffix { get; init; }
        public bool IsGovUkSuffix { get; init; }
        public bool IsCustomText { get; init; }
        public string? ResolvedText { get; init; }
    }

    public static SuffixRenderInfo GetSuffixRenderInfo(InputSuffixOptions? inputSuffix, ResourceManager? resourceManager = null)
    {
        if (inputSuffix == null || string.IsNullOrWhiteSpace(inputSuffix.Text))
        {
            return new SuffixRenderInfo
            {
                HasSuffix = false,
                IsGovUkSuffix = false,
                IsCustomText = false,
                ResolvedText = null
            };
        }

        var isGovUkSuffix = inputSuffix.Type == InputSuffixType.GovUkDefault;
        var isCustomText = inputSuffix.Type == InputSuffixType.CustomText;

        string? resolvedText = null;
        if (isGovUkSuffix)
        {
            var rm = resourceManager ?? StaticTextResource.ResourceManager;
            resolvedText = rm.GetString(inputSuffix.Text) ?? inputSuffix.Text;
        }
        else if (isCustomText)
        {
            resolvedText = inputSuffix.Text;
        }

        return new SuffixRenderInfo
        {
            HasSuffix = true,
            IsGovUkSuffix = isGovUkSuffix,
            IsCustomText = isCustomText,
            ResolvedText = resolvedText
        };
    }

    public static string GetInputCssClasses(SuffixRenderInfo suffixInfo, InputWidthType? inputWidth, string? customCssClasses, bool hasError)
    {
        var cssClasses = new List<string> { "govuk-input" };

        if (inputWidth.HasValue)
        {
            cssClasses.Add(GovUkCssHelper.GetInputWidthCssClass(inputWidth.Value));
        }

        if (!string.IsNullOrWhiteSpace(customCssClasses))
        {
            cssClasses.Add(customCssClasses);
        }

        if (hasError)
        {
            cssClasses.Add("govuk-input--error");
        }

        if (suffixInfo.IsCustomText)
        {
            cssClasses.Add("govuk-!-display-inline-block");
            cssClasses.Add("govuk-!-vertical-align-middle");
        }

        return string.Join(" ", cssClasses);
    }

    public static string GetContainerCssClasses(SuffixRenderInfo suffixInfo)
    {
        return string.Empty;
    }

    public static string GetSuffixCssClasses(SuffixRenderInfo suffixInfo)
    {
        if (suffixInfo.IsGovUkSuffix)
        {
            return "govuk-input__suffix";
        }

        if (suffixInfo.IsCustomText)
        {
            return "govuk-body govuk-!-display-inline-block govuk-!-margin-left-2 govuk-!-margin-bottom-0 govuk-!-vertical-align-middle";
        }

        return string.Empty;
    }

    public static string GetSuffixElementTag(SuffixRenderInfo suffixInfo)
    {
        return suffixInfo.IsCustomText ? "p" : "div";
    }

    public static bool ShouldUseInputWrapper(SuffixRenderInfo suffixInfo)
    {
        return suffixInfo.IsGovUkSuffix;
    }
}