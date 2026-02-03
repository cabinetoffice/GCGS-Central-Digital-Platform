using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

[Authorize]
[Route("organisation/{organisationSlug}/applications")]
public class ApplicationsController(
    IApplicationService applicationService,
    ApplicationRegistryClient apiClient) : Controller
{
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        string? organisationSlug,
        Guid? cdpOrganisationId,
        string? category,
        string? status,
        string? search,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (cdpOrganisationId.HasValue)
        {
            try
            {
                var org = await apiClient.ByCdpGuidAsync(cdpOrganisationId.Value, ct);
                return RedirectToAction(nameof(Index), new { organisationSlug = org.Slug, category, status, search });
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

        var viewModel = await applicationService.GetApplicationsViewModelAsync(organisationSlug, category, status, search, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet]
    [Route("{applicationSlug}/enable")]
    public async Task<IActionResult> Enable(string organisationSlug, string applicationSlug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(applicationSlug))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetEnableApplicationViewModelAsync(organisationSlug, applicationSlug, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost]
    [Route("{applicationSlug}/enable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(string organisationSlug, string applicationSlug, bool confirm, CancellationToken ct)
    {
        if (!confirm)
        {
            ModelState.AddModelError(nameof(confirm), "You must confirm to enable this application");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await applicationService.GetEnableApplicationViewModelAsync(organisationSlug, applicationSlug, ct);
            return View(viewModel);
        }

        var success = await applicationService.EnableApplicationAsync(organisationSlug, applicationSlug, ct);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(EnableSuccess), new { organisationSlug, applicationSlug });
    }

    [HttpGet]
    [Route("{applicationSlug}/enable-success")]
    public async Task<IActionResult> EnableSuccess(string organisationSlug, string applicationSlug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(applicationSlug))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetEnableSuccessViewModelAsync(organisationSlug, applicationSlug, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet]
    [Route("{applicationSlug}")]
    public async Task<IActionResult> Details(string organisationSlug, string applicationSlug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(applicationSlug))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetApplicationDetailsViewModelAsync(organisationSlug, applicationSlug, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
