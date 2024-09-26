using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationApprovalModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select an option")]
    public bool Approval { get; set; }

    [BindProperty]
    public string? Comments { get; set; }

    public async Task<IActionResult> OnGet(Guid organisationId)
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(organisationId);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost(Guid organisationId)
    {
        if (!ModelState.IsValid)
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(organisationId);
            return Page();
        }

        if (UserDetails.PersonId != null)
        {
            SupportOrganisationInfo orgInfo = new SupportOrganisationInfo(
                Approval,
                UserDetails.PersonId.Value,
                Comments ?? ""
            );

            await organisationClient.SupportUpdateOrganisationAsync(organisationId, new SupportUpdateOrganisation(
                orgInfo,
                SupportOrganisationUpdateType.Review
            ));
        }
        else
        {
            return Redirect("/");
        }

        return RedirectToPage("Organisations", new { type = "buyer" });
    }
}