using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Services;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

[Authorize]
public class ApplicationsController(IApplicationService applicationService) : Controller
{
    // TODO: Get from authenticated user claims
    private const int CurrentOrgId = 1;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var viewModel = await applicationService.GetApplicationsViewModelAsync(CurrentOrgId, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
