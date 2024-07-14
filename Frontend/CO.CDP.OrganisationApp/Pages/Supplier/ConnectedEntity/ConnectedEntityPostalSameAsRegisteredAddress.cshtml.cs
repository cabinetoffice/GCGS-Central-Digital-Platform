using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

[Authorize]
public class ConnectedEntityPostalSameAsRegisteredAddressModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? SameAsRegiseterdAddress { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntityQuestion", new { Id, ConnectedEntityId });
        }

        SameAsRegiseterdAddress = selected.HasValue ? selected : AreSameAddress(state.RegisteredAddress, state.PostalAddress);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntityQuestion", new { Id, ConnectedEntityId });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (SameAsRegiseterdAddress == true)
        {
            state.PostalAddress = state.RegisteredAddress;
            session.Set(Session.ConnectedPersonKey, state);

            //TODO 5: Next page link??;
            return RedirectToPage("ConnectedEntityDeclaration", new { Id });
        }

        return RedirectToPage("ConnectedEntityAddress",
            new { Id, ConnectedEntityId, AddressType = Constants.AddressType.Registered, UkOrNonUk = "uk" });
    }

    private (bool valid, ConnectedEntityState state) ValidatePage()
    {
        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (cp == null ||
            cp.SupplierOrganisationId != Id ||
            (ConnectedEntityId.HasValue && cp.ConnectedEntityId.HasValue && cp.ConnectedEntityId != ConnectedEntityId))
        {
            return (false, new());
        }
        return (true, cp);
    }

    private static bool AreSameAddress(ConnectedEntityState.Address? registeredAddress, ConnectedEntityState.Address? postalAddress)
    {
        return registeredAddress != null
                && postalAddress != null
                && registeredAddress.AddressLine1 == postalAddress.AddressLine1
                && registeredAddress.TownOrCity == postalAddress.TownOrCity
                && registeredAddress.Postcode == postalAddress.Postcode
                && registeredAddress.Country == postalAddress.Country;
    }
}