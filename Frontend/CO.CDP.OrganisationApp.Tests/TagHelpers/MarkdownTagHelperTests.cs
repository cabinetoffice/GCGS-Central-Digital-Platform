using CO.CDP.OrganisationApp.TagHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using static CO.CDP.OrganisationApp.Tests.TagHelpers.TagHelperTestKit;

namespace CO.CDP.OrganisationApp.Tests.TagHelpers;

public class MarkdownTagHelperTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public MarkdownTagHelperTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
    }

    [Fact]
    public async Task MarkdownTagHelper_ShouldRenderHtml_FromInlineMarkdown()
    {
        var tagHelper = new MarkdownTagHelper(_mockEnvironment.Object);
        var markdown = "# Heading\nThis is **bold** text.";

        var result = await CallTagHelper("markdown", markdown, new TagHelperAttributeList(), tagHelper, true);

        result.Should().Be("<h1 class=\"govuk-heading-xl\">Heading</h1>\n<p class=\"govuk-body\">This is <strong>bold</strong> text.</p>\n");
    }

    [Fact]
    public async Task MarkdownTagHelper_ShouldApplyGOVUKClasses()
    {
        var tagHelper = new MarkdownTagHelper(_mockEnvironment.Object);
        var markdown = "# Title\n## Subtitle\n### Small Title\n- Item 1\n1. Item 1";

        var result = await CallTagHelper("markdown", markdown, new TagHelperAttributeList(), tagHelper, true);

        result.Should().Be(
            "<h1 class=\"govuk-heading-xl\">Title</h1>\n" +
            "<h2 class=\"govuk-heading-l\">Subtitle</h2>\n" +
            "<h3 class=\"govuk-heading-m\">Small Title</h3>\n" +
            "<ul class=\"govuk-list govuk-list--bullet\">\n" +
            "<li>Item 1</li>\n</ul>\n" +
            "<ol class=\"govuk-list govuk-list--number\">\n" +
            "<li>Item 1</li>\n</ol>\n");
    }
}
