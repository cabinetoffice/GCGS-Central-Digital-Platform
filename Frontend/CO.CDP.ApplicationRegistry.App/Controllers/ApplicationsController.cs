using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

[Authorize]
public class ApplicationsController(
    IApplicationService applicationService,
    ApplicationRegistryClient apiClient) : Controller
{
    public async Task<IActionResult> Index(string? organisationSlug, Guid? cdpOrganisationId, CancellationToken ct)
    {
        if (cdpOrganisationId.HasValue)
        {
            try
            {
                var org = await apiClient.ByCdpGuidAsync(cdpOrganisationId.Value, ct);
                return RedirectToAction(nameof(Index), new { organisationSlug = org.Slug });
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

        if (string.IsNullOrEmpty(organisationSlug))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetApplicationsViewModelAsync(organisationSlug, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
