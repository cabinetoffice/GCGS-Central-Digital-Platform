using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

public class ChildOrganisationSuccessPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationSuccessPage> logger,
    IAuthorizationService authorizationService)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationSuccessPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IAuthorizationService _authorizationService =
        authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

    [BindProperty(SupportsGet = true)] public required Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public required Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string OrganisationName { get; set; } = string.Empty;

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, Id, new IsBuyerRequirement());
        if (!authResult.Succeeded)
        {
            _logger.LogWarning("User is not authorised to access child organisation success page for parent ID {OrganisationId}.", Id);
            return Redirect("/page-not-found");
        }

        if (ChildId == Guid.Empty)
        {
            return RedirectToPage("/Error");
        }

        try
        {
            ChildOrganisation = await _organisationClient.GetOrganisationAsync(ChildId);

            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Child organisation not found for ChildId: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            OrganisationName = ChildOrganisation.Name;
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while retrieving child organisation details";
            var cdpException = new CdpExceptionLogging(errorMessage, "LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }

        return Page();
    }
}