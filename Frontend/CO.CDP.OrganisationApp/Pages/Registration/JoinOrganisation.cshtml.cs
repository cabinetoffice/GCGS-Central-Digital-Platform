using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select an option")]
    public bool Join { get; set; }

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
            if (Join == true)
            {
                await organisationClient.CreateJoinRequestAsync(organisationId, new CreateOrganisationJoinRequest(
                    personId: UserDetails.PersonId.Value
                ));

                return Redirect("/registration/" + organisationId + "/join-organisation/success");
            }

            return Redirect("/registration/has-companies-house-number");
        }
        else
        {
            return Redirect("/");
        }
    }
}