using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierQualificationNameModel(
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please enter person or awarding body.")]
    public string? QualificationName { get; set; }

    public IActionResult OnGet()
    {
        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        QualificationName = qa.Name;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        qa.Name = QualificationName;
        tempDataService.Put(Qualification.TempDataKey, qa);

        return RedirectToPage("SupplierQualificationCheckAnswer", new { Id });        
    }
}