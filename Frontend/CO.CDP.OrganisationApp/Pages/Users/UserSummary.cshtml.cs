using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
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

    public ICollection<Organisation.WebApiClient.Person> Persons { get; set; } = [];

    public ICollection<PersonInviteModel> PersonInvites { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another user")]
    public bool? HasPerson { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        SignedInPersonId = UserDetails.PersonId;

        try
        {
            Persons = await organisationClient.GetOrganisationPersonsAsync(Id);
            PersonInvites = await organisationClient.GetOrganisationPersonInvitesAsync(Id);
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
}