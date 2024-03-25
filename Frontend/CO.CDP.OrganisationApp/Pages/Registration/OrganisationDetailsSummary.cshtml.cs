using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.SupplierRegistration;

public class OrganisationDetailsSummaryModel : PageModel
{
    private readonly ILogger<OrganisationDetailsSummaryModel> _logger;

    public OrganisationDetailsSummaryModel(ILogger<OrganisationDetailsSummaryModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}
