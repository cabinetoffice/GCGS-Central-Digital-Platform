using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityIndividualCategoryModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityIndividualCategory_SelectCategoryError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public ConnectedEntityIndividualAndTrustCategoryType? ConnectedEntityCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }
    [BindProperty]
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public bool RegisteredWithCh { get; set; }

    public IActionResult OnGet()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }

        ConnectedEntityCategory = state.ConnectedEntityIndividualAndTrustCategoryType;
        ConnectedEntityType = state.ConnectedEntityType;
        RegisteredWithCh = state.SupplierHasCompanyHouseNumber ?? false;
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null || !ModelState.IsValid)
        {
            return Page();
        }

        state.ConnectedEntityIndividualAndTrustCategoryType = ConnectedEntityCategory;
        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage(GetRedirectLinkPageName(state), new { Id });
    }

    public static Dictionary<string, string> ConnectedEntityCategoryOption => new()
    {
        { "PSC", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityIndividualCategory_CategoryOption_PSC },
        { "Director", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityIndividualCategory_CategoryOption_PSC_Director},
        { "IndividualWithSignificantControl", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityIndividualCategory_CategoryOption_PSC_Individual}
    };

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityPscDetails";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityPscDetails";
                        break;
                }
                break;
        }

        return redirectPage;
    }
}