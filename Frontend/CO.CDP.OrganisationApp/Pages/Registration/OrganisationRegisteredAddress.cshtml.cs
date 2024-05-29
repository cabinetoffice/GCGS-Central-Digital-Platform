using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationRegisteredAddressModel(ISession session) : PageModel
{
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
    public string? Country { get; set; }

    public void OnGet()
    {
        var details = VerifySession();

        AddressLine1 = details.OrganisationAddressLine1;
        AddressLine2 = details.OrganisationAddressLine2;
        TownOrCity = details.OrganisationCityOrTown;
        Postcode = details.OrganisationPostcode;
        Country = details.OrganisationCountry;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationAddressLine1 = AddressLine1 ?? registrationDetails.OrganisationAddressLine1;
        registrationDetails.OrganisationAddressLine2 = AddressLine2 ?? registrationDetails.OrganisationAddressLine2;
        registrationDetails.OrganisationCityOrTown = TownOrCity ?? registrationDetails.OrganisationCityOrTown;
        registrationDetails.OrganisationPostcode = Postcode ?? registrationDetails.OrganisationPostcode;
        registrationDetails.OrganisationCountry = Country ?? registrationDetails.OrganisationCountry;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (registrationDetails.OrganisationType == OrganisationType.Buyer)
        {
            return RedirectToPage("BuyerOrganisationType");
        }
        else
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}