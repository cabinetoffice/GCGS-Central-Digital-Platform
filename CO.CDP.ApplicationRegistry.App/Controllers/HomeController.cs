using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.Authentication.Services;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

[Authorize]
public class HomeController(
    IApplicationService applicationService) : Controller
{
    // TODO: Get from authenticated user claims
    private const int CurrentOrgId = 1;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var viewModel = await applicationService.GetHomeViewModelAsync(CurrentOrgId, ct);
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
