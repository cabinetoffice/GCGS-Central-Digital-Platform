using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierQualificationAwardingBodyModel(
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please enter person or awarding body.")]
    public string? AwardedByPersonOrBodyName { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public Guid? QualificationId { get; set; }

    public IActionResult OnGet()
    {
        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        QualificationId = qa.Id;
        AwardedByPersonOrBodyName = qa.AwardedByPersonOrBodyName;
        return Page();
    }

    public IActionResult OnPost(Guid? QualificationId)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        qa.AwardedByPersonOrBodyName = AwardedByPersonOrBodyName;
        tempDataService.Put(Qualification.TempDataKey, qa);

        return RedirectToPage("SupplierQualificationAwardedDate", new { Id });
    }
}

public class Qualification
{
    public const string TempDataKey = "QualificationTempData";
    public Guid? Id { get; set; }
    public string? AwardedByPersonOrBodyName { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? DateAwarded { get; set; }
}