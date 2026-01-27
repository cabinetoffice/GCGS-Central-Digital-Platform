using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

[Authorize]
public class HomeController(
    IApplicationService applicationService,
    ApplicationRegistryClient apiClient) : Controller
{
    public async Task<IActionResult> Index(string? organisationSlug, Guid? cdpOrganisationId, CancellationToken ct)
    {
        // If arriving via GUID route, lookup org and redirect to slug-based URL
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

        // Normal slug-based flow
        if (string.IsNullOrEmpty(organisationSlug))
        {
            return NotFound();
        }

        var viewModel = await applicationService.GetHomeViewModelAsync(organisationSlug, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
