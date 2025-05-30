using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CO.CDP.OrganisationApp.Tests.TagHelpers;

public static class TagHelperTestKit
{
    public static async Task<string> CallTagHelper(
        string tagName,
        string htmlContent,
        TagHelperAttributeList attributes,
        TagHelper tagHelper,
        bool async = false)
    {
        var tagHelperContext = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput(
            tagName,
            attributes,
            (_, _) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(htmlContent);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
        if(async)
        {
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);
        } else
        {
            tagHelper.Process(tagHelperContext, tagHelperOutput);
        }

        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}