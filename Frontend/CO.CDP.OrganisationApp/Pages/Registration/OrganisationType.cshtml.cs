using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OrganisationTypeModel : PageModel
{
    private readonly ILogger<OrganisationTypeModel> _logger;
    public bool HasError { get; set; }

    public OrganisationTypeModel(ILogger<OrganisationTypeModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        HasError = false;
    }

    public IActionResult OnPost(string[] supplierType)
    {
        if (supplierType == null || supplierType.Length == 0)
        {
            HasError = true;
            return Page();
        }

        return RedirectToPage("./OrganisationDetails");
    }
}