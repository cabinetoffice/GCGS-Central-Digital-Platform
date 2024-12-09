using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
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
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_JoinOrganisation_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? UserWantsToJoin { get; set; }

    [BindProperty]
    [RequiredIf(nameof(UserWantsToJoin), true, ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_JoinOrganisation_ConfirmValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? UserConfirmation { get; set; }

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
            if (UserWantsToJoin.GetValueOrDefault())
            {
                try
                {
                    var joinRequestStatus = await organisationClient.CreateJoinRequestAsync(OrganisationDetails.Id,
                        new CreateOrganisationJoinRequest(
                            personId: UserDetails.PersonId.Value
                        ));

                    if (joinRequestStatus.Status == OrganisationJoinRequestStatus.Pending)
                    {
                        if (joinRequestStatus.IsNewRequest)
                        {
                            return Redirect("/registration/" + identifier + "/join-organisation/success");
                        }
                        else
                        {
                            tempDataService.Put(FlashMessageTypes.Failure, new FlashMessage(StaticTextResource.OrganisationRegistration_JoinOrganisation_PendingMemberOfOrganisation, null, StaticTextResource.Global_Important));
                            return Page();
                        }
                    }
                }
                catch (ApiException<OrganisationWebApiClient.ProblemDetails>)
                {
                    tempDataService.Put(FlashMessageTypes.Failure, new FlashMessage(StaticTextResource.OrganisationRegistration_JoinOrganisation_AlreadyMemberOfOrganisation, null, StaticTextResource.Global_Important));
                    return Page();
                }
                finally
                {
                    SessionContext.Remove(Session.RegistrationDetailsKey);
                }
            }

            return Redirect("/organisation-selection");
        }
        else
        {
            return Redirect("/");
        }
    }
}