using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages
{
    public class DetailsModel(ISearchService searchService) : PageModel
    {
        public SearchResult? Result { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Result = await searchService.GetByIdAsync(id);
            if (Result == null)
            {
                return RedirectToPage("/NotFound");
            }
            return Page();
        }
    }
}
