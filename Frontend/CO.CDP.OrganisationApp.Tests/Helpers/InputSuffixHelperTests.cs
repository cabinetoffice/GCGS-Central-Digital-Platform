using CO.CDP.OrganisationApp.Helpers;
using CO.CDP.OrganisationApp.Models;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.Helpers;

public class InputSuffixHelperTests
{
    [Fact]
    public void GetSuffixRenderInfo_WithNullInputSuffix_ReturnsNoSuffix()
    {
        var result = InputSuffixHelper.GetSuffixRenderInfo(null);

        result.HasSuffix.Should().BeFalse();
        result.IsGovUkSuffix.Should().BeFalse();
        result.IsCustomText.Should().BeFalse();
        result.ResolvedText.Should().BeNull();
    }

    [Fact]
    public void GetSuffixRenderInfo_WithGovUkDefault_ReturnsGovUkSuffix()
    {
        var inputSuffix = new InputSuffixOptions
        {
            Type = InputSuffixType.GovUkDefault,
            Text = "TestKey"
        };

        var result = InputSuffixHelper.GetSuffixRenderInfo(inputSuffix);

        result.HasSuffix.Should().BeTrue();
        result.IsGovUkSuffix.Should().BeTrue();
        result.IsCustomText.Should().BeFalse();
        result.ResolvedText.Should().Be("TestKey"); // Falls back to key when no resource found
    }

    [Fact]
    public void GetSuffixRenderInfo_WithCustomText_ReturnsCustomText()
    {
        var inputSuffix = new InputSuffixOptions
        {
            Type = InputSuffixType.CustomText,
            Text = "days"
        };

        var result = InputSuffixHelper.GetSuffixRenderInfo(inputSuffix);

        result.HasSuffix.Should().BeTrue();
        result.IsGovUkSuffix.Should().BeFalse();
        result.IsCustomText.Should().BeTrue();
        result.ResolvedText.Should().Be("days");
    }

    [Fact]
    public void GetInputCssClasses_WithError_IncludesErrorClass()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { HasSuffix = false };

        var result = InputSuffixHelper.GetInputCssClasses(suffixInfo, InputWidthType.Width2, "custom-class", hasError: true);

        result.Should().Contain("govuk-input");
        result.Should().Contain("govuk-input--error");
        result.Should().Contain("custom-class");
    }

    [Fact]
    public void GetContainerCssClasses_WithCustomText_ReturnsEmpty()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsCustomText = true };

        var result = InputSuffixHelper.GetContainerCssClasses(suffixInfo);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetInputCssClasses_WithCustomText_IncludesInlineBlockClasses()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsCustomText = true };

        var result = InputSuffixHelper.GetInputCssClasses(suffixInfo, InputWidthType.Width2, null, false);

        result.Should().Contain("govuk-input");
        result.Should().Contain("govuk-!-display-inline-block");
        result.Should().Contain("govuk-!-vertical-align-middle");
    }

    [Fact]
    public void GetSuffixCssClasses_WithCustomText_IncludesInlineBlockClasses()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsCustomText = true };

        var result = InputSuffixHelper.GetSuffixCssClasses(suffixInfo);

        result.Should().Contain("govuk-body");
        result.Should().Contain("govuk-!-display-inline-block");
        result.Should().Contain("govuk-!-vertical-align-middle");
        result.Should().Contain("govuk-!-margin-left-2");
        result.Should().Contain("govuk-!-margin-bottom-0");
    }

    [Fact]
    public void GetSuffixElementTag_WithCustomText_ReturnsP()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsCustomText = true };

        var result = InputSuffixHelper.GetSuffixElementTag(suffixInfo);

        result.Should().Be("p");
    }

    [Fact]
    public void GetSuffixElementTag_WithGovUkSuffix_ReturnsDiv()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsGovUkSuffix = true };

        var result = InputSuffixHelper.GetSuffixElementTag(suffixInfo);

        result.Should().Be("div");
    }

    [Fact]
    public void ShouldUseInputWrapper_WithGovUkSuffix_ReturnsTrue()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsGovUkSuffix = true };

        var result = InputSuffixHelper.ShouldUseInputWrapper(suffixInfo);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldUseInputWrapper_WithCustomText_ReturnsFalse()
    {
        var suffixInfo = new InputSuffixHelper.SuffixRenderInfo { IsCustomText = true };

        var result = InputSuffixHelper.ShouldUseInputWrapper(suffixInfo);

        result.Should().BeFalse();
    }
}