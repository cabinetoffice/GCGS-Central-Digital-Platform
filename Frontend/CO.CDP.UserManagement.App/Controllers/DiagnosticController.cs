using CO.CDP.UserManagement.App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[AllowAnonymous]
public class DiagnosticController(IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        return View(new DiagnosticViewModel(
            [
                new DiagnosticServiceViewModel("User Management API", configuration["UserManagementApi:BaseUrl"]),
                new DiagnosticServiceViewModel("Sirsi Service", configuration["SirsiService:ServiceBaseUrl"]),
                new DiagnosticServiceViewModel("FTS Service", configuration["FtsService:ServiceBaseUrl"])
            ]));
    }
}
