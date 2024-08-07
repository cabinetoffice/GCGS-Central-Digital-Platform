using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityIndividualCategoryModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select the category that best describes the 'connected person'")]
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
        { "PSC", "person with significant control" },
        { "Director", "director or individual with the same responsibilities"},
        { "IndividualWithSignificantControl", "any other individual with significant influence or control"}
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