using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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

        registrationDetails.OrganisationAddressLine1 = AddressLine1;
        registrationDetails.OrganisationAddressLine2 = AddressLine2;
        registrationDetails.OrganisationCityOrTown = TownOrCity;
        registrationDetails.OrganisationPostcode = Postcode;
        registrationDetails.OrganisationCountry = Country;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationDetailsSummary");
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception("Session not found");

        return registrationDetails;
    }
}