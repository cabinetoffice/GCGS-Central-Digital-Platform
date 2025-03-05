using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationModel(
    IOrganisationClient organisationClient,
    ISession session,
    IFlashMessageService flashMessageService) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_JoinOrganisation_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? UserWantsToJoin { get; set; }

    [BindProperty]
    [RequiredIf(nameof(UserWantsToJoin), true, ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_JoinOrganisation_ConfirmValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? UserConfirmation { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var jor = session.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest);

            if (jor == null || jor.OrganisationId != Id)
            {
                return Redirect("/page-not-found");
            }            

            OrganisationDetails = await organisationClient.LookupOrganisationAsync(jor.OrganisationName, "");

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var jor = session.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest);

        if (jor == null || jor.OrganisationId != Id)
        {
            return Redirect("/page-not-found");
        }

        OrganisationDetails = await organisationClient.LookupOrganisationAsync(jor.OrganisationName, "");

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
                    var joinRequestStatus = await organisationClient.CreateJoinRequestAsync(
                        OrganisationDetails.Id,
                        new CreateOrganisationJoinRequest(personId: UserDetails.PersonId.Value));

                    if (joinRequestStatus != null)
                    {
                        if ((!joinRequestStatus.RequestCreated) && (joinRequestStatus.Status == OrganisationJoinRequestStatus.Pending))
                        {
                            flashMessageService.SetFlashMessage(
                                FlashMessageType.Failure,
                                StaticTextResource.OrganisationRegistration_JoinOrganisation_PendingMemberOfOrganisation,
                                null,
                                StaticTextResource.Global_Important);
                            return Page();
                        }

                        return Redirect("/registration/" + Id + "/join-organisation/success");
                    }
                }
                catch (ApiException<OrganisationWebApiClient.ProblemDetails>)
                {
                    flashMessageService.SetFlashMessage(FlashMessageType.Important, StaticTextResource.OrganisationRegistration_JoinOrganisation_AlreadyMemberOfOrganisation);
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