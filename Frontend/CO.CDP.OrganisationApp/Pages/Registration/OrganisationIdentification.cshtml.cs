using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration
{
    public class OrganisationIdentificationModel : PageModel
    {
        private readonly ILogger<OrganisationIdentificationModel> _logger;
        public bool HasError { get; set; }
        public OrganisationIdentificationModel(ILogger<OrganisationIdentificationModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            HasError = false;
        }

        public IActionResult OnPost(string organisationIdentification, IDictionary<string, string> organisationNumbers)
        {

            if (organisationIdentification == "otherNone" || ValidateOrganisationNumber(organisationIdentification, organisationNumbers))
            {
                return RedirectToPage("./OrganisationRegisteredAddress");
            }

            HasError = true;
            return Page();
        }

        private bool ValidateOrganisationNumber(string organisationIdentification, IDictionary<string, string> organisationNumbers)
        {
            if (organisationNumbers.TryGetValue(organisationIdentification, out var number))
            {
                return !string.IsNullOrEmpty(number);
            }
            HasError = true;
            return false;
        }

    }
}
