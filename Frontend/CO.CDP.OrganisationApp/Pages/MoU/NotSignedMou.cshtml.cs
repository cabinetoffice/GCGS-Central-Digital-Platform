using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.MoU;

public class NotSignedMouModel(
    IOrganisationClient organisationClient,
    ILogger<NotSignedMouModel> logger,
    ISession session,
    IAuthorizationService authorizationService)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Origin { get; set; }

    [BindProperty] public string OrganisationName { get; set; } = string.Empty;

    [BindProperty] public bool CanSignMou { get; private set; }

    [BindProperty] public UserDetails? UserDetails { get; private set; }

    public string BackLinkUrl { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        if (OrganisationId == Guid.Empty)
        {
            logger.LogWarning("Organisation ID not provided or invalid");
            return RedirectToPage("/Error");
        }

        UserDetails = session.Get<UserDetails>(Session.UserDetailsKey);

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(OrganisationId);
            if (organisation == null)
            {
                logger.LogWarning($"Organisation with ID {OrganisationId} not found");
                return RedirectToPage("/Error");
            }

            OrganisationName = organisation.Name;

            var isEditorResult = await authorizationService.AuthorizeAsync(User, OrgScopeRequirement.Editor);
            CanSignMou = isEditorResult.Succeeded;

            BackLinkUrl = Origin switch
            {
                "organisation-home" => $"/organisation/{OrganisationId}/home",
                _ => $"/organisation/{OrganisationId}"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error retrieving organisation with ID {OrganisationId}");
            return RedirectToPage("/Error");
        }

        return Page();
    }
}