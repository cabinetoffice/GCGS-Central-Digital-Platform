using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration
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
