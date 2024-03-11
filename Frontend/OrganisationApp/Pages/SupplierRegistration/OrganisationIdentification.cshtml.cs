using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrganisationApp.Pages.SupplierRegistration
{
    public class OrganisationIdentificationModel : PageModel
    {
        private readonly ILogger<OrganisationIdentificationModel> _logger;

        public OrganisationIdentificationModel(ILogger<OrganisationIdentificationModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
