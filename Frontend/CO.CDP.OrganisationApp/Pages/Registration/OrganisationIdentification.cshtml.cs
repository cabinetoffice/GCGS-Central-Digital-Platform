using CO.CDP.OrganisationApp.CustomeValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration
{
    public class OrganisationIdentificationModel : PageModel
    {
        [BindProperty]
        [DisplayName("Organisation Type")]
        [Required(ErrorMessage = "Please select your organisation type")]
        public string? OrganisationType { get; set; }

        [BindProperty]
        [DisplayName("Companies House")]
        public string? CompaniesHouse { get; set; }

        [BindProperty]
        [DisplayName("Companies House Number")]
        [RequiredIf("OrganisationType", "CHN")]
        public string? CompaniesHouseNumber { get; set; }

        [BindProperty]
        [DisplayName("Dun & Bradstreet")]
        public string? DunBradstreet { get; set; }

        [BindProperty]
        [DisplayName("Dun & Bradstreet Number")]
        [RequiredIf("OrganisationType", "DUN")]
        public string? DunBradstreetNumber { get; set; }

        [BindProperty]
        [DisplayName("Charity Commission for England & Wales")]
        public string? CharityCommissionEnglandWales { get; set; }

        [BindProperty]
        [DisplayName("Charity Commission for England & Wales Number")]
        [RequiredIf("OrganisationType", "CCEW")]
        public string? CharityCommissionEnglandWalesNumber { get; set; }

        [BindProperty]
        [DisplayName("Office of the Scottish Charity Regulator (OSCR)")]
        public string? OfficeOfScottishCharityRegulator { get; set; }

        [BindProperty]
        [DisplayName("Office of the Scottish Charity Regulator (OSCR) Number")]
        [RequiredIf("OrganisationType", "OSCR")]
        public string? OfficeOfScottishCharityRegulatorNumber { get; set; }

        [BindProperty]
        [DisplayName("The Charity Commission for Northren Ireland")]
        public string? CharityCommissionNorthernIreland { get; set; }

        [BindProperty]
        [DisplayName("The Charity Commission for Northren Ireland Number")]
        [RequiredIf("OrganisationType", "CCNI")]
        public string? CharityCommissionNorthernIrelandNumber { get; set; }

        [BindProperty]
        [DisplayName("National health Service Organisations Registry")]
        public string? NationalHealthServiceOrganisationsRegistry { get; set; }

        [BindProperty]
        [DisplayName("National health Service Organisations Registry Number")]
        [RequiredIf("OrganisationType", "NHOR")]
        public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

        [BindProperty]
        [DisplayName("Department For Education")]
        public string? DepartmentForEducation { get; set; }

        [BindProperty]
        [DisplayName("Department For Education Number")]
        [RequiredIf("OrganisationType", "DFE")]
        public string? DepartmentForEducationNumber { get; set; }

        [BindProperty]
        [DisplayName("Other / None")]
        public string? Other { get; set; }

        [BindProperty]
        [DisplayName("Other / None Number")]
        [RequiredIf("OrganisationType", "Other")]
        public string? OtherNumber { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("./OrganisationRegisteredAddress");
        }
    }

}
