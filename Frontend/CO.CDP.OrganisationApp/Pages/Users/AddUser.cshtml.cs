using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class AddUserModel(
    ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Guid? PersonInviteId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.User_FirstName_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? FirstName { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.User_LastName_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? LastName { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.User_Email_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Email { get; set; }

    [BindProperty]
    public bool? IsAdmin { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.User_Role_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Role { get; set; }

    public PersonInviteState? PersonInviteStateData;

    public IActionResult OnGet()
    {
        PersonInviteStateData = session.Get<PersonInviteState>(PersonInviteState.TempDataKey) ?? null;

        if (PersonInviteStateData != null)
        {
            InitModel(PersonInviteStateData);
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        PersonInviteStateData = session.Get<PersonInviteState>(PersonInviteState.TempDataKey) ?? new PersonInviteState();

        PersonInviteStateData = UpdateFields(PersonInviteStateData);

        PersonInviteStateData = UpdateScopes(PersonInviteStateData);

        session.Set(PersonInviteState.TempDataKey, PersonInviteStateData);

        return RedirectToPage("UserCheckAnswers", new { Id });
    }

    public PersonInviteState UpdateFields(PersonInviteState state)
    {
        if (!FirstName.IsNullOrEmpty())
        {
            state.FirstName = FirstName ?? "";
        }

        if (!LastName.IsNullOrEmpty())
        {
            state.LastName = LastName ?? "";
        }

        if (!Email.IsNullOrEmpty())
        {
            state.Email = Email ?? "";
        }

        return state;
    }

    public PersonInviteState UpdateScopes(PersonInviteState state)
    {
        var scopes = state.Scopes;

        if (scopes.IsNullOrEmpty())
        {
            scopes = [];
        }

        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Admin)) scopes.Remove(OrganisationPersonScopes.Admin);
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Editor)) scopes.Remove(OrganisationPersonScopes.Editor);
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Viewer)) scopes.Remove(OrganisationPersonScopes.Viewer);

        switch (Role)
        {
            case OrganisationPersonScopes.Admin:
                scopes?.Add(OrganisationPersonScopes.Admin);
                break;
            case OrganisationPersonScopes.Editor:
                scopes?.Add(OrganisationPersonScopes.Editor);
                break;
            default:
                scopes?.Add(OrganisationPersonScopes.Viewer);
                break;
        }

        if (!scopes?.Contains(OrganisationPersonScopes.Responder) ?? false)
        {
            scopes?.Add(OrganisationPersonScopes.Responder);
        }

        state.Scopes = scopes;

        return state;
    }

    public void InitModel(PersonInviteState state)
    {
        FirstName = state.FirstName;
        LastName = state.LastName;
        Email = state.Email;

        if (!state.Scopes.IsNullOrEmpty())
        {
            if (state.Scopes != null && state.Scopes.Contains(OrganisationPersonScopes.Admin))
            {
                IsAdmin = true;
            }

            if (state.Scopes != null && state.Scopes.Contains(OrganisationPersonScopes.Editor))
            {
                Role = OrganisationPersonScopes.Editor;
            }
            else
            {
                Role = OrganisationPersonScopes.Viewer;
            }
        }
    }
}