using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class OrganisationRegisteredAddressModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationAddressPage;
    public override ISession SessionContext => session;

    [BindProperty]
    [DisplayName("Address line 1")]
    [Required(ErrorMessage = "Enter your address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Address line 2 (optional)")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [DisplayName("Town or city")]
    [Required(ErrorMessage = "Enter your town or city")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [DisplayName("Postcode")]
    [Required(ErrorMessage = "Enter your postcode")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; } = "United Kingdom";

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        if (RegistrationDetails.OrganisationCountry == "United Kingdom")
        {
            AddressLine1 = RegistrationDetails.OrganisationAddressLine1;
            AddressLine2 = RegistrationDetails.OrganisationAddressLine2;
            TownOrCity = RegistrationDetails.OrganisationCityOrTown;
            Postcode = RegistrationDetails.OrganisationPostcode;
            Country = RegistrationDetails.OrganisationCountry;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationAddressLine1 = AddressLine1 ?? RegistrationDetails.OrganisationAddressLine1;
        RegistrationDetails.OrganisationAddressLine2 = AddressLine2 ?? RegistrationDetails.OrganisationAddressLine2;
        RegistrationDetails.OrganisationCityOrTown = TownOrCity ?? RegistrationDetails.OrganisationCityOrTown;
        RegistrationDetails.OrganisationPostcode = Postcode ?? RegistrationDetails.OrganisationPostcode;
        RegistrationDetails.OrganisationRegion = "";
        RegistrationDetails.OrganisationCountry = Country ?? RegistrationDetails.OrganisationCountry;

        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }

        if (RegistrationDetails.OrganisationType == OrganisationType.Buyer)
        {
            return RedirectToPage("BuyerOrganisationType");
        }

        return RedirectToPage("OrganisationDetailsSummary");
    }
}