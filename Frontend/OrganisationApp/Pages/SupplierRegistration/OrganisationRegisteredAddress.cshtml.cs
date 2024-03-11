using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrganisationApp.Pages.SupplierRegistration
{
    public class OrganisationRegisteredAddressModel : PageModel
    {
        private readonly ILogger<OrganisationRegisteredAddressModel> _logger;

        public OrganisationRegisteredAddressModel(ILogger<OrganisationRegisteredAddressModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
