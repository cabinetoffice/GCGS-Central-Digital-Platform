using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration
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
