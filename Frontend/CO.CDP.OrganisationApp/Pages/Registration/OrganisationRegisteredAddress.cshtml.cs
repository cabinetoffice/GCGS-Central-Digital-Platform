using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CO.CDP.OrganisationApp.Models;
using System.Net.Mail;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OrganisationRegisteredAddressModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Address line 1")]
    [Required(ErrorMessage = "Enter your address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Address line 2")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [DisplayName("City or town")]
    [Required(ErrorMessage = "Enter your city or town")]
    public string? CityOrTown { get; set; }

    [BindProperty]
    [DisplayName("Postcode or ZIP Code")]
    [Required(ErrorMessage = "Enter your postcode or zip Code")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        AddressLine1 = registrationDetails.OrganisationAddressLine1;
        AddressLine2 = registrationDetails.OrganisationAddressLine2;
        CityOrTown = registrationDetails.OrganisationCityOrTown;
        Postcode = registrationDetails.OrganisationPostcode;
        Country = registrationDetails.OrganisationCountry;
    }

    public IActionResult OnPost() {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationAddressLine1 = AddressLine1;
        registrationDetails.OrganisationAddressLine2 = AddressLine2;
        registrationDetails.OrganisationCityOrTown = CityOrTown;
        registrationDetails.OrganisationPostcode = Postcode;
        registrationDetails.OrganisationCountry = Country;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationDetailsSummary");

    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }
        return registrationDetails;
    }
}
