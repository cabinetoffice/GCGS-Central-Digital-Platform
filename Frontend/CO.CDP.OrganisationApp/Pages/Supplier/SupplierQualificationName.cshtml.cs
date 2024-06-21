using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierQualificationNameModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please enter person or awarding body.")]
    public string? Name { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid? qualificationId)
    {
        try
        {
            if (qualificationId == null)
            {
                var composed = await organisationClient.GetComposedOrganisation(Id);

                if (composed != null && composed.SupplierInfo.CompletedQualification)
                {
                    var qualification = composed.SupplierInfo.Qualifications.FirstOrDefault(a => a.Id == qualificationId);

                    if (qualification != null)
                    {
                        Name = qualification.Name;
                    }
                }
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

        try
        {
            session.Set("AwardedDate", Name);

        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}

