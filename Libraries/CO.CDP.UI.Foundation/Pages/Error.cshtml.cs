using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.UI.Foundation.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(IConfiguration configuration) : PageModel
{
    public required string TraceId { get; set; }
    public new int StatusCode { get; set; }
    public string? FeedbackUrl { get; private set; }

    public IActionResult OnGet(int? statusCode = null)
    {
        StatusCode = statusCode ?? 500;
        Response.StatusCode = StatusCode;
        TraceId = HttpContext.TraceIdentifier;

        var organisationAppUrl = configuration.GetValue<string>("OrganisationAppUrl");
        if (!string.IsNullOrEmpty(organisationAppUrl))
        {
            FeedbackUrl = new Uri(new Uri(organisationAppUrl), "/provide-feedback-and-contact/").ToString();
        }

        return Page();
    }
}