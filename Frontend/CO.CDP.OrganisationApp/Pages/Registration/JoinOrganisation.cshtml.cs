using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationModel(
    IOrganisationClient organisationClient,
    ISession session,
    ITempDataService tempDataService) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select an option")]
    public bool Join { get; set; }

    public FlashMessage NotificationBannerAlreadyMemberOfOrganisation { get { return new FlashMessage("You are already a member of this organisation. <a class='govuk-notification-banner__link' href='/organisation/" + OrganisationDetails?.Id + "'>View Organisation</a>"); } }

    public async Task<IActionResult> OnGet(string identifier)
    {
        try
        {
            OrganisationDetails = await organisationClient.LookupOrganisationAsync(string.Empty, $"{identifier}");
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost(string identifier)
    {
        OrganisationDetails = await organisationClient.LookupOrganisationAsync(string.Empty, $"{identifier}");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (UserDetails.PersonId != null && OrganisationDetails != null)
        {
            if (Join == true)
            {
                try
                {
                    await organisationClient.CreateJoinRequestAsync(OrganisationDetails.Id,
                        new CreateOrganisationJoinRequest(
                            personId: UserDetails.PersonId.Value
                        ));
                }
                catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
                {
                    tempDataService.Put(FlashMessageTypes.Important, NotificationBannerAlreadyMemberOfOrganisation);
                    return Page();
                }

                return Redirect("/registration/" + identifier + "/join-organisation/success");
            }

            return Redirect("/registration/has-companies-house-number");
        }
        else
        {
            return Redirect("/");
        }
    }
}