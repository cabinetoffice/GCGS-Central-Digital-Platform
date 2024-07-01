using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class TradeAssuranceBodyModel(
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the name of the awarding person or body")]
    public string? AwardedByPersonOrBodyName { get; set; }

    public Guid? TradeAssuranceId { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public IActionResult OnGet()
    {
        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        TradeAssuranceId = ta.Id;
        AwardedByPersonOrBodyName = ta.AwardedByPersonOrBodyName;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        ta.AwardedByPersonOrBodyName = AwardedByPersonOrBodyName;
        tempDataService.Put(TradeAssurance.TempDataKey, ta);

        if (RedirectToCheckYourAnswer == true)
        {
            return RedirectToPage("TradeAssuranceCheckAnswer", new { Id });
        }
        else
        {
            return RedirectToPage("TradeAssuranceReferenceNumber", new { Id });
        }
    }
}

public class TradeAssurance
{
    public const string TempDataKey = "TradeAssuranceTempData";
    public Guid? Id { get; set; }
    public string? AwardedByPersonOrBodyName { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTimeOffset? DateAwarded { get; set; }
}