using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class AddressTypeQuestionModel(
    ISession session, IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasUkAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Constants.AddressType AddressType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);

            if ((composed.SupplierInfo.CompletedRegAddress && AddressType == Constants.AddressType.Registered)
                || (composed.SupplierInfo.CompletedPostalAddress && AddressType == Constants.AddressType.Postal))
            {
                var address = composed.Organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.AsApiClientAddressType());

                HasUkAddress = address != null && address.CountryName == Country.UnitedKingdom;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }


    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage(HasUkAddress == true ? "AddressUk" : "AddressNonUk", new { Id, AddressType = AddressType.ToString().ToLower() });
    }
}