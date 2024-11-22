using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityDirectorResidencyModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityDirectorResidency_SelectCountryError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? DirectorLocation { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersIndividualOrTrust"
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModel(state, true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? "ConnectedEntityCheckAnswersIndividualOrTrust"
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModel(state);

        if (!ModelState.IsValid) return Page();

        state.DirectorLocation = DirectorLocation;

        session.Set(Session.ConnectedPersonKey, state);

        var redirectPage = (RedirectToCheckYourAnswer == true
                            ? "ConnectedEntityCheckAnswersIndividualOrTrust"
                            : "ConnectedEntityAddress");

        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId, AddressType = AddressType.Registered, UkOrNonUk = "uk" });
    }

    private void InitModel(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = string.Format(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityDirectorResidency_Heading, state.FirstName);

        if (reset)
        {
            DirectorLocation = state.DirectorLocation;
        }
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
}
