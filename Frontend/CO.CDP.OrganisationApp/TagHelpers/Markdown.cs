using Markdig;
using Markdig.Parsers;
using Markdig.Syntax;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CO.CDP.OrganisationApp.TagHelpers;

[HtmlTargetElement("markdown")]
public class MarkdownTagHelper : TagHelper
{
    private readonly IWebHostEnvironment _env;

    public MarkdownTagHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string? FileName { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        string markdownContent;

        if (!string.IsNullOrEmpty(FileName))
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Pages", FileName);

            if (File.Exists(filePath))
            {
                markdownContent = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                throw new FileNotFoundException($"The specified Markdown file '{FileName}' was not found.", filePath);
            }
        }
        else
        {
            markdownContent = (await output.GetChildContentAsync()).GetContent();
        }

        var htmlContent = Markdown.ToHtml(markdownContent);

        output.Content.SetHtmlContent(htmlContent);
    }
}

public class AddClassToBlockProcessor : Markdig.Parsers.BlockParser
{
    public override void Process(BlockProcessor processor)
    {
        if (processor.Block is HeadingBlock heading)
        {
            // Add class to headings
            heading.Attributes.AddClass("my-heading-class");
        }
        else if (processor.Block is ParagraphBlock paragraph)
        {
            // Add class to paragraphs
            paragraph.Attributes.AddClass("my-paragraph-class");
        }
    }
}