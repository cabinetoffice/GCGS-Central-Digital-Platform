using Microsoft.AspNetCore.Mvc;
using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Controllers;

public class ApplicationsController : Controller
{
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(ILogger<ApplicationsController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // TODO: Replace with API calls via Refitter client
        var model = new ApplicationsViewModel
        {
            OrganisationName = "Cabinet Office",
            EnabledApplications =
            [
                new ApplicationViewModel
                {
                    Slug = "find-a-tender",
                    Name = "Find a Tender",
                    Description = "Search and manage public procurement opportunities. Create, publish and manage contract notices for goods, services and works.",
                    Category = "Procurement",
                    IsEnabled = true,
                    UsersAssigned = 3,
                    RolesAvailable = 2
                },
                new ApplicationViewModel
                {
                    Slug = "contracts-finder",
                    Name = "Contracts Finder",
                    Description = "Publish contract awards and find information about contracts with the government and public sector. Search historical contract data.",
                    Category = "Procurement",
                    IsEnabled = true,
                    UsersAssigned = 2,
                    RolesAvailable = 3
                },
                new ApplicationViewModel
                {
                    Slug = "spend-data-service",
                    Name = "Spend Data Service",
                    Description = "Upload and publish government spending data. View and analyse procurement spend across departments and suppliers.",
                    Category = "Data & Analytics",
                    IsEnabled = true,
                    UsersAssigned = 3,
                    RolesAvailable = 2
                }
            ],
            AvailableApplications =
            [
                new ApplicationViewModel
                {
                    Slug = "digital-marketplace",
                    Name = "Digital Marketplace",
                    Description = "Buy cloud technology and specialist services for digital projects. Access G-Cloud and Digital Outcomes frameworks.",
                    Category = "Procurement",
                    IsEnabled = false,
                    RolesAvailable = 4
                },
                new ApplicationViewModel
                {
                    Slug = "procurement-pipeline",
                    Name = "Procurement Pipeline",
                    Description = "View and manage upcoming procurement opportunities. Plan and coordinate procurement activities across your organisation.",
                    Category = "Procurement",
                    IsEnabled = false,
                    RolesAvailable = 3
                },
                new ApplicationViewModel
                {
                    Slug = "supplier-information-service",
                    Name = "Supplier Information Service",
                    Description = "Access verified supplier information and due diligence data. Streamline supplier onboarding and verification processes.",
                    Category = "Data & Analytics",
                    IsEnabled = false,
                    RolesAvailable = 2
                }
            ]
        };

        return View(model);
    }
}
