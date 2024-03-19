using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

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