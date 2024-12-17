using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using AddressType = CO.CDP.Organisation.WebApiClient.AddressType;
using ProblemDetails = CO.CDP.Organisation.WebApiClient.ProblemDetails;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumChangeAddressModel(IOrganisationClient organisationClient) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }


    public async Task<IActionResult> OnGet()
    {
        var consortium = await organisationClient.GetOrganisationAsync(Id);
        if (consortium == null) return Redirect("/page-not-found");

        SetupAddress(true);

        var postalAddress = consortium.Addresses.FirstOrDefault(x => x.Type == AddressType.Postal);

        if (postalAddress == null) return Redirect("/page-not-found");

        if ((Address.IsNonUkAddress && postalAddress.Country != Country.UKCountryCode)
            || (!Address.IsNonUkAddress && postalAddress.Country == Country.UKCountryCode))
        {
            Address.AddressLine1 = postalAddress.StreetAddress;
            Address.TownOrCity = postalAddress.Locality;
            Address.Postcode = postalAddress.PostalCode;
            Address.Country = postalAddress.Country;
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
            ICollection<OrganisationAddress> addresses = [
                            new OrganisationAddress(
                            streetAddress: Address.AddressLine1,
                            postalCode: Address.Postcode,
                            locality: Address.TownOrCity,
                            countryName: Address.CountryName,
                            country: Address.Country,
                            type: AddressType.Postal,
                            region: null)];

            await organisationClient.UpdateOrganisationAddresses(Id, addresses);
        }
        catch (ApiException<ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("ConsortiumOverview", new { Id });

    }

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        if (Address.IsNonUkAddress)
        {
            Address.Heading = StaticTextResource.Consortium_ConsortiumAddress_NonUk_Heading;
            Address.AddressHint = string.Empty;
        }
        else
        {
            Address.Heading = StaticTextResource.Consortium_ConsortiumAddress_Heading;
            Address.AddressHint = string.Empty;
        }

        Address.NonUkAddressLink = $"/consortium/{Id}/change-address/non-uk";
    }
}