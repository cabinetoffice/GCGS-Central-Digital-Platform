using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class TradeAssuranceReferenceNumberModel(
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the trade assurance reference number")]
    public string? ReferenceNumber { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public IActionResult OnGet()
    {
        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        ReferenceNumber = ta.ReferenceNumber;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        ta.ReferenceNumber = ReferenceNumber;
        tempDataService.Put(TradeAssurance.TempDataKey, ta);

        if (RedirectToCheckYourAnswer == true)
        {
            return RedirectToPage("TradeAssuranceCheckAnswer", new { Id });
        }
        else
        {
            return RedirectToPage("TradeAssuranceAwardedDate", new { Id });
        }
    }
}