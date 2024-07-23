using CO.CDP.OrganisationApp.Constants;
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
    public bool? DifferentThanRegiseterdAddress { get; set; }

    public string? Caption { get; set; }

    public string? OrganisationName { get; set; }

    public bool? IsNonUkAddress { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        var sameAddress = state.RegisteredAddress?.AreSameAddress(state.PostalAddress);

        DifferentThanRegiseterdAddress = selected.HasValue ? selected : (sameAddress.HasValue ? !sameAddress : sameAddress);

        InitModal(state);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (DifferentThanRegiseterdAddress == false)
        {
            state.PostalAddress = state.RegisteredAddress;
            session.Set(Session.ConnectedPersonKey, state);

            //TODO 5: Next page link??;
            return RedirectToPage("ConnectedEntityLawRegister", new { Id, ConnectedEntityId });
        }

        return RedirectToPage("ConnectedEntityAddress",
            new { Id, ConnectedEntityId, AddressType = AddressType.Postal, UkOrNonUk = "uk" });
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
        
    private void InitModal(ConnectedEntityState state)
    {
        OrganisationName = state.OrganisationName;
        Caption = state.GetCaption();
        IsNonUkAddress = state.RegisteredAddress?.IsNonUk;
    }
}