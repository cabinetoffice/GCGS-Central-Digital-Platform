using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class AddUserModel(
    ISession session) : PageModel
{
    public const string ScopeAdmin = "ADMIN";
    public const string ScopeEditor = "EDITOR";
    public const string ScopeViewer = "VIEWER";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Guid? PersonInviteId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Firstname required")]
    public string? FirstName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Lastname required")]
    public string? LastName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Email required")]
    public string? Email { get; set; }

    [BindProperty]
    public bool? IsAdmin { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Role required")]
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

        if (scopes != null && scopes.Contains(ScopeAdmin)) scopes.Remove(ScopeAdmin);
        if (scopes != null && scopes.Contains(ScopeEditor)) scopes.Remove(ScopeEditor);
        if (scopes != null && scopes.Contains(ScopeViewer)) scopes.Remove(ScopeViewer);

        if (IsAdmin == true)
        {
            scopes?.Add(ScopeAdmin);
        }

        if (Role == ScopeEditor)
        {
            scopes?.Add(ScopeEditor);
        }
        else
        {
            scopes?.Add(ScopeViewer);
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
            if (state.Scopes != null && state.Scopes.Contains(ScopeAdmin))
            {
                IsAdmin = true;
            }

            if (state.Scopes != null && state.Scopes.Contains(ScopeEditor))
            {
                Role = ScopeEditor;
            }
            else
            {
                Role = ScopeViewer;
            }
        }
    }
}