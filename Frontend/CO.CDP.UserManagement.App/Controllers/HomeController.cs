using CO.CDP.UserManagement.App.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize(Policy = PolicyNames.OrganisationOwnerOrAdmin)]
public class HomeController : Controller
{
    public IActionResult Index(Guid? id)
    {
        if (!id.HasValue)
            return NotFound();

        return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id = id.Value });
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
