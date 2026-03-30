using CO.CDP.UserManagement.App.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize(Policy = PolicyNames.OrganisationOwnerOrAdmin)]
public class HomeController(UserManagementClient apiClient) : Controller
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
    [Route("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View();
    }

    [AllowAnonymous]
    [Route("/page-not-found")]
    public IActionResult PageNotFound()
    {
        Response.StatusCode = 404;
        return View();
    }

}
