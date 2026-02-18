using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.WebApiClient;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize]
public class HomeController(
    IApplicationService applicationService,
    UserManagementClient apiClient) : Controller
{
    public async Task<IActionResult> Index(string? organisationSlug, Guid? cdpOrganisationId, CancellationToken ct)
    {
        if (cdpOrganisationId.HasValue)
        {
            try
            {
                var org = await apiClient.ByCdpGuidAsync(cdpOrganisationId.Value, ct);
                return RedirectToAction(nameof(UsersController.Index), "Users", new { organisationSlug = org.Slug });
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

        return RedirectToAction(nameof(UsersController.Index), "Users", new { organisationSlug });
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
