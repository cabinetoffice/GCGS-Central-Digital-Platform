using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrganisationApp.Pages.SupplierRegistration;

public class OrganisationTypeModel : PageModel
{
    private readonly ILogger<OrganisationTypeModel> _logger;

    public OrganisationTypeModel(ILogger<OrganisationTypeModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}