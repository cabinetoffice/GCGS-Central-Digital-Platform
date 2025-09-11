using CO.CDP.UI.Foundation.Pages;
using System.Text;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class DiagnosticPage(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DiagnosticPageBase(httpContextAccessor, configuration)
{
    protected override Task AddAdditionalContent(StringBuilder sb)
    {
        sb.Append("<h2 class=\"heading-l\">Connected Services</h2>");
        sb.Append("<dl class=\"summary-list\">");

        var apiUrl = Configuration["CommercialToolsApi:ServiceUrl"];
        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Commercial Tools API</dt>");
        sb.Append("<dd class=\"summary-list__value\">");
        if (!string.IsNullOrEmpty(apiUrl))
        {
            sb.Append("<strong class=\"tag tag--green\">Configured</strong>");
        }
        else
        {
            sb.Append("<strong class=\"tag tag--red\">Not configured</strong>");
        }
        sb.Append("</dd>");
        sb.Append("</div>");

        sb.Append("</dl>");

        return Task.CompletedTask;
    }

}