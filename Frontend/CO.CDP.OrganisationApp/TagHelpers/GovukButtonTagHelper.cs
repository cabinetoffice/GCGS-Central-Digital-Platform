using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace CO.CDP.OrganisationApp.TagHelpers;

public class GovukButtonTagHelper : TagHelper
{
    public string? Class { get; set; }
    public string Type { get; set; } = "submit";
    public bool PreventDoubleClick { get; set; } = true;
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.Attributes.SetAttribute("type", Type);
        output.Attributes.SetAttribute("data-module", "govuk-button");
        output.AddClass("govuk-button", HtmlEncoder.Default);

        if (!string.IsNullOrEmpty(Class))
        {
            output.AddClass(Class.Trim(), HtmlEncoder.Default);
        }

        if (PreventDoubleClick)
        {
            output.Attributes.SetAttribute("data-prevent-double-click", "true");
        }

        output.Content.SetHtmlContent(output.GetChildContentAsync().Result.GetContent());
    }
}
