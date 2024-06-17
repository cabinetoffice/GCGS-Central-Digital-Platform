using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class OrganisationNonUKAddressModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationNonUKAddressPage;
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
    [DisplayName("Country, State or Province (optional)")]
    public string? Region { get; set; }

    [BindProperty]
    [DisplayName("Postal or Zip code")]
    [Required(ErrorMessage = "Enter your postcode")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        if (RegistrationDetails.OrganisationCountry != Constants.Country.UnitedKingdom)
        {
            AddressLine1 = RegistrationDetails.OrganisationAddressLine1;
            AddressLine2 = RegistrationDetails.OrganisationAddressLine2;
            TownOrCity = RegistrationDetails.OrganisationCityOrTown;
            Region = RegistrationDetails.OrganisationRegion;
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

        RegistrationDetails.OrganisationAddressLine1 = AddressLine1;
        RegistrationDetails.OrganisationAddressLine2 = AddressLine2;
        RegistrationDetails.OrganisationCityOrTown = TownOrCity;
        RegistrationDetails.OrganisationRegion = Region;
        RegistrationDetails.OrganisationPostcode = Postcode;
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