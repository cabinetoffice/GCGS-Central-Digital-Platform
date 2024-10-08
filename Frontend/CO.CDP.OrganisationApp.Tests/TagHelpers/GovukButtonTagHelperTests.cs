using CO.CDP.OrganisationApp.TagHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace CO.CDP.OrganisationApp.Tests.TagHelpers;
public class GovukButtonTagHelperTests
{
    [Fact]
    public void GovukButtonTagHelper_ShouldRenderButtonWithDefaults()
    {
        var attributes = new TagHelperAttributeList();

        var result = callTagHelper("Click me", attributes);

        result.Should().Be("<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public void GovukButtonTagHelper_ShouldRenderButton_WhenCustomClassIsSet()
    {
        var attributes = new TagHelperAttributeList();

        var result = callTagHelper("Click me", attributes, @class: "custom-class");

        result.Should().Be("<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button custom-class\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public void GovukButtonTagHelper_ShouldRenderButton_WhenMultipleCustomClassesAreSet()
    {
        var attributes = new TagHelperAttributeList();

        var result = callTagHelper("Click me", attributes, @class: "custom-class another-class many-classes");

        result.Should().Be("<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button custom-class another-class many-classes\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public void GovukButtonTagHelper_ShouldRenderButton_WhenTypeIsSet()
    {
        var attributes = new TagHelperAttributeList();

        var result = callTagHelper("Click me", attributes, type: "button");

        result.Should().Be("<button type=\"button\" data-module=\"govuk-button\" class=\"govuk-button\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public void GovukButtonTagHelper_ShouldRenderButton_WhenDoubleClickIsSetToFalse()
    {
        var attributes = new TagHelperAttributeList();

        var result = callTagHelper("Click me", attributes, preventDoubleClick: false);

        result.Should().Be("<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button\">Click me</button>");
    }

    public string callTagHelper(string htmlContent, TagHelperAttributeList attributes, string? @class = null, string? type = null, bool? preventDoubleClick = null)
    {
        var tagHelper = new GovukButtonTagHelper();

        if (!string.IsNullOrEmpty(@class))
        {
            tagHelper.Class = @class;
        }

        if (!string.IsNullOrEmpty(type))
        {
            tagHelper.Type = type;
        }

        if (preventDoubleClick != null)
        {
            tagHelper.PreventDoubleClick = (bool)preventDoubleClick;
        }

        var tagHelperContext = new TagHelperContext(
                        attributes,
                        new Dictionary<object, object>(),
                        Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("govuk-button",
            attributes,
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(htmlContent);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
        tagHelper.Process(tagHelperContext, tagHelperOutput);

        var writer = new System.IO.StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
