using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

// [Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel
        {
            OrganisationName = "Cabinet Office",
            Stats = new DashboardStats
            {
                ApplicationsEnabled = 3,
                TotalUsers = 12,
                ActiveAssignments = 8,
                RolesAssigned = 5
            },
            EnabledApplications =
            [
                new EnabledApplicationViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Find a Tender",
                    Description = "Search and manage public procurement opportunities. Create, publish and manage contract notices.",
                    UsersAssigned = 3,
                    RolesAvailable = 2
                },
                new EnabledApplicationViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Contracts Finder",
                    Description = "Publish contract awards and find information about contracts with the government and public sector.",
                    UsersAssigned = 2,
                    RolesAvailable = 3
                },
                new EnabledApplicationViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Spend Data Service",
                    Description = "Upload and publish government spending data. View and analyse procurement spend across departments.",
                    UsersAssigned = 3,
                    RolesAvailable = 2
                }
            ]
        };

        return View(model);
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
