using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationInternationalIdentificationCountryModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]

    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Country_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Country_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Country { get; set; } = string.Empty;

    public void OnGet()
    {
        Country = HttpContext.Request.Query["country"].ToString();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("OrganisationInternationalIdentification", new { country = Country, id = Id });
    }
}