using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AddressType = CO.CDP.Organisation.WebApiClient.AddressType;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationRegisteredAddressModel(OrganisationWebApiClient.IOrganisationClient organisationClient) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public bool? RedirectToOverview { get; set; }

    public async Task<IActionResult> OnGet()
    {
        SetupAddress(true);

        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null) return Redirect("/page-not-found");

        var regsiteredAddress = organisation.Addresses.FirstOrDefault(x => x.Type == AddressType.Registered);

        if (regsiteredAddress == null) return Redirect("/page-not-found");

        if ((Address.IsNonUkAddress && regsiteredAddress.Country != Country.UKCountryCode)
            || (!Address.IsNonUkAddress && regsiteredAddress.Country == Country.UKCountryCode))
        {
            Address.AddressLine1 = regsiteredAddress.StreetAddress;
            Address.TownOrCity = regsiteredAddress.Locality;
            Address.Postcode = regsiteredAddress.PostalCode;
            Address.Country = regsiteredAddress.Country;
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        SetupAddress();
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);

            ICollection<OrganisationWebApiClient.OrganisationAddress> addresses = [
                            new OrganisationWebApiClient.OrganisationAddress(
                            streetAddress: Address.AddressLine1,
                            postalCode: Address.Postcode,
                            locality: Address.TownOrCity,
                            countryName: Address.CountryName,
                            country: Address.Country,
                            type: AddressType.Registered,
                            region: null)];

            await organisationClient.UpdateOrganisationAddresses(Id, addresses);
            if (RedirectToOverview == true)
                return RedirectToPage("OrganisationOverview", new { Id });
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (OrganisationWebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("OrganisationOverview", new { Id });

    }

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        Address.Heading = Address.IsNonUkAddress ?
            "Enter your organisation's registered non-UK address" : "Enter your organisation's registered address";

        Address.AddressHint = "The address registered with Companies House, or the principal address the business conducts its activities. For example, a head office.";

        Address.NonUkAddressLink = $"/organisation/{Id}/address/non-uk{(RedirectToOverview == true ? "?frm-overview" : "")}";
    }
}
