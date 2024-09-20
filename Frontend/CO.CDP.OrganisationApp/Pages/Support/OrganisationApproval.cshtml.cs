using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationApprovalModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty(SupportsGet = true)]
    public Boolean? Approval { get; init; }

    [BindProperty(SupportsGet = true)]
    public string? Comments { get; init; }

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
        await organisationClient.ReviewOrganisationAsync(new ReviewOrganisation(
            Approval ?? false,
            UserDetails.PersonId ?? Guid.Empty,
            Comments ?? "",
            organisationId
            ));

        return RedirectToPage("Organisations", new { type = "buyer" });
    }
}