using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class CompanyHouseNumberQuestionModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Please select your organisation type")]
    public string? YesNo { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
    }
}
