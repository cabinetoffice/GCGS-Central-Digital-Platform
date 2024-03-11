using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrganisationApp.Pages.SupplierRegistration
{

    public class YourDetailsModel : PageModel
    {
        private readonly ILogger<YourDetailsModel> _logger;

        public YourDetailsModel(ILogger<YourDetailsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}