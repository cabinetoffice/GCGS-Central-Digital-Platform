using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.WebApiClient;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize]
[Route("organisation/{organisationSlug}/applications")]
public class ApplicationsController(
    IApplicationService applicationService,
    UserManagementClient apiClient) : Controller
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
    [Route("{application}/enable")]
    public async Task<IActionResult> Enable(string organisationSlug, string application, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(application))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetEnableApplicationViewModelAsync(organisationSlug, application, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost]
    [Route("{application}/enable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(string organisationSlug, string application, bool confirm, CancellationToken ct)
    {
        if (!confirm)
        {
            ModelState.AddModelError(nameof(confirm), "You must confirm to enable this application");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await applicationService.GetEnableApplicationViewModelAsync(organisationSlug, application, ct);
            return View(viewModel);
        }

        var success = await applicationService.EnableApplicationAsync(organisationSlug, application, ct);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(EnableSuccess), new { organisationSlug, application });
    }

    [HttpGet]
    [Route("{application}/enable-success")]
    public async Task<IActionResult> EnableSuccess(string organisationSlug, string application, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(application))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetEnableSuccessViewModelAsync(organisationSlug, application, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet]
    [Route("{application}")]
    public async Task<IActionResult> Details(string organisationSlug, string application, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(application))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetApplicationDetailsViewModelAsync(organisationSlug, application, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet]
    [Route("{application}/disable")]
    public async Task<IActionResult> Disable(string organisationSlug, string application, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug) || string.IsNullOrEmpty(application))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetDisableApplicationViewModelAsync(organisationSlug, application, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost]
    [Route("{application}/disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(string organisationSlug, string application, bool confirm, CancellationToken ct)
    {
        if (!confirm)
        {
            ModelState.AddModelError(nameof(confirm), "You must confirm to disable this application");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await applicationService.GetDisableApplicationViewModelAsync(organisationSlug, application, ct);
            return View(viewModel);
        }

        var success = await applicationService.DisableApplicationAsync(organisationSlug, application, ct);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { organisationSlug, application });
    }
}
