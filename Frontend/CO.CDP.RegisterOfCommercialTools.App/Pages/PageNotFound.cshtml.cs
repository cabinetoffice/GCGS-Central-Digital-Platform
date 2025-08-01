using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.UI.Foundation.Pages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class PageNotFoundModel(NotFoundPage notFoundPage) : PageModel
{
    public string NotFoundHtml { get; private set; } = string.Empty;

    public void OnGet()
    {
        NotFoundHtml = notFoundPage.Render();
    }
}