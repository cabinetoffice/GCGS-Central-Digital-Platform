using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.SupplierRegistration 
{ 

    public class OrganisationDetailModel : PageModel
    {

        private readonly ILogger<OrganisationDetailModel> _logger;

        public OrganisationDetailModel(ILogger<OrganisationDetailModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
        }
    }
}
