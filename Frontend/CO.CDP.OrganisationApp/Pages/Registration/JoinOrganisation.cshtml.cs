using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationModel(
    IOrganisationClient organisationClient,
    ISession session,
    IFlashMessageService flashMessageService) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.OrganisationRegistration_JoinOrganisation_ValidationErrorMessage))]
    public bool Join { get; set; }

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
            if (Join)
            {
                try
                {
                    await organisationClient.CreateJoinRequestAsync(OrganisationDetails.Id,
                        new CreateOrganisationJoinRequest(
                            personId: UserDetails.PersonId.Value
                        ));
                }
                catch (ApiException<OrganisationWebApiClient.ProblemDetails>)
                {
                    flashMessageService.SetFlashMessage(FlashMessageType.Important, ErrorMessagesList.AlreadyMemberOfOrganisation);
                    return Page();
                }

                SessionContext.Remove(Session.RegistrationDetailsKey);

                return Redirect("/registration/" + identifier + "/join-organisation/success");
            }

            return Redirect("/organisation-selection");
        }
        else
        {
            return Redirect("/");
        }
    }
}