using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class UserSummaryModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public Guid? SignedInPersonId { get; set; }

    [BindProperty]
    public Guid? JoinRequestId { get; set; }

    [BindProperty]
    public Guid? PersonId { get; set; }

    public ICollection<CO.CDP.Organisation.WebApiClient.Person> Persons { get; set; } = [];

    public ICollection<PersonInviteModel> PersonInvites { get; set; } = [];
    public ICollection<JoinRequestLookUp> OrganisationJoinRequests { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another user")]
    public bool? HasPerson { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        SignedInPersonId = UserDetails.PersonId;

        try
        {
            var getPersonsTask = organisationClient.GetOrganisationPersonsAsync(Id);
            var getPersonInvitesTask = organisationClient.GetOrganisationPersonInvitesAsync(Id);
            var getOrganisationJoinRequestsTask = organisationClient.GetOrganisationJoinRequests(Id, null);

            await Task.WhenAll(getPersonsTask, getPersonInvitesTask, getOrganisationJoinRequestsTask);

            Persons = getPersonsTask.Result;
            PersonInvites = getPersonInvitesTask.Result;
            OrganisationJoinRequests = getOrganisationJoinRequestsTask.Result;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasPerson = selected;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return await OnGet(HasPerson);
        }

        if (HasPerson == true)
        {
            return Redirect("add-user");
        }

        return Redirect("/organisation/" + Id);
    }

    public async Task<IActionResult> OnGetJoinRequest(Guid reqid, Guid personid, OrganisationJoinRequestStatus decision)
    {
        try
        {
            var updateJoinRequest = new UpdateJoinRequest(UserDetails.PersonId!.Value, decision);

            await organisationClient.UpdateOrganisationJoinRequest(Id, reqid, updateJoinRequest);

            var redirectUrl = decision == OrganisationJoinRequestStatus.Accepted
            ? $"/organisation/{Id}/users/{personid}/change-role?handler=person"
            : $"/organisation/{Id}/users/user-summary";

            return Redirect(redirectUrl);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}