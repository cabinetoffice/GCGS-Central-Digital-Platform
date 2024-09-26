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
    public Boolean? Approval { get; set; }

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
        SupportOrganisationInfo orgInfo = new SupportOrganisationInfo(
            Approval ?? false,
            UserDetails.PersonId ?? Guid.Empty,
            Comments ?? ""
        );

        await organisationClient.SupportUpdateOrganisationAsync(organisationId, new SupportUpdateOrganisation(
            orgInfo,
            SupportOrganisationUpdateType.Review
        ));

        return RedirectToPage("Organisations", new { type = "buyer" });
    }
}