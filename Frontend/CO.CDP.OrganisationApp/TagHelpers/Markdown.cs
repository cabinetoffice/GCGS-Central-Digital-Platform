using Markdig;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.RegularExpressions;

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

        htmlContent = Regex.Replace(htmlContent, @"<p([^>]*)>", "<p $1 class=\"govuk-body\">");
        htmlContent = Regex.Replace(htmlContent, @"<h1([^>]*)>", "<h1 $1 class=\"govuk-heading-xl\">");
        htmlContent = Regex.Replace(htmlContent, @"<h2([^>]*)>", "<h2 $1 class=\"govuk-heading-l\">");
        htmlContent = Regex.Replace(htmlContent, @"<h3([^>]*)>", "<h3 $1 class=\"govuk-heading-m\">");
        htmlContent = Regex.Replace(htmlContent, @"<h4([^>]*)>", "<h4 $1 class=\"govuk-heading-s\">");
        htmlContent = Regex.Replace(htmlContent, @"<h5([^>]*)>", "<h5 $1 class=\"govuk-heading-s\">");
        htmlContent = Regex.Replace(htmlContent, @"<h6([^>]*)>", "<h6 $1 class=\"govuk-heading-s\">");
        htmlContent = Regex.Replace(htmlContent, @"<a([^>]*)>", "<a $1 class=\"govuk-link\">");
        htmlContent = Regex.Replace(htmlContent, @"<ul([^>]*)>", "<ul $1 class=\"govuk-list govuk-list--bullet\">");
        htmlContent = Regex.Replace(htmlContent, @"<ol([^>]*)>", "<ol $1 class=\"govuk-list govuk-list--number\">");

        output.Content.SetHtmlContent(htmlContent);
    }
}