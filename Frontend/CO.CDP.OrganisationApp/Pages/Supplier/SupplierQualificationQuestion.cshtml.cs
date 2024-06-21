using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierQualificationQuestionModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasRelevantQualifications { get; set; }

    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);

            if (composed.SupplierInfo.CompletedQualification)
            {
                HasRelevantQualifications = true;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (HasRelevantQualifications == true)
        {
            return RedirectToPage("SupplierQualificationAwardingBody", new { Id });
        }
        else
        {
            return RedirectToPage("SupplierBasicInformation", new { Id });
        }
    }
}
