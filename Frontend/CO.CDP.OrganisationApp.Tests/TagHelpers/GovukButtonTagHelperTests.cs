using CO.CDP.OrganisationApp.TagHelpers;
using FluentAssertions;
using static CO.CDP.OrganisationApp.Tests.TagHelpers.TagHelperTestKit;

namespace CO.CDP.OrganisationApp.Tests.TagHelpers;

public class GovukButtonTagHelperTests
{
    [Fact]
    public async Task GovukButtonTagHelper_ShouldRenderButtonWithDefaults()
    {
        var result = await CallTagHelper("govuk-button", "Click me", [], new GovukButtonTagHelper());

        result.Should()
            .Be(
                "<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public async Task GovukButtonTagHelper_ShouldRenderButton_WhenCustomClassIsSet()
    {
        var result = await CallTagHelper("govuk-button", "Click me", [], new GovukButtonTagHelper
        {
            Class = "custom-class"
        });

        result.Should()
            .Be(
                "<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button custom-class\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public async Task GovukButtonTagHelper_ShouldRenderButton_WhenMultipleCustomClassesAreSet()
    {
        var result = await CallTagHelper("govuk-button", "Click me", [], new GovukButtonTagHelper
        {
            Class = "custom-class another-class many-classes"
        });

        result.Should()
            .Be(
                "<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button custom-class another-class many-classes\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public async Task GovukButtonTagHelper_ShouldRenderButton_WhenTypeIsSet()
    {
        var result = await CallTagHelper("govuk-button", "Click me", [], new GovukButtonTagHelper
        {
            Type = "button"
        });

        result.Should()
            .Be(
                "<button type=\"button\" data-module=\"govuk-button\" class=\"govuk-button\" data-prevent-double-click=\"true\">Click me</button>");
    }

    [Fact]
    public async Task GovukButtonTagHelper_ShouldRenderButton_WhenDoubleClickIsSetToFalse()
    {
        var result = await CallTagHelper("govuk-button", "Click me", [], new GovukButtonTagHelper
        {
            PreventDoubleClick = false
        });

        result.Should()
            .Be("<button type=\"submit\" data-module=\"govuk-button\" class=\"govuk-button\">Click me</button>");
    }
}