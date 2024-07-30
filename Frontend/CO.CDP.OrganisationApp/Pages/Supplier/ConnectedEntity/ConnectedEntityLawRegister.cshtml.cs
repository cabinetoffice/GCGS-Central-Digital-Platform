using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityLawRegisterModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter what is organisation's legal form?")]
    public string? LegalForm { get; set; }

    [BindProperty]
    [DisplayName("Which law enforces it?")]
    [Required(ErrorMessage = "Enter which law enforces it?")]
    public string? LawRegistered { get; set; }

    [BindProperty]
    public ConnectedEntityType? ConnectedEntityType { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string? Caption { get; set; }

    public string? LegalFormDisplayText { get; set; }

    public string? BackPageLink { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                    ? "ConnectedEntityCheckAnswersOrganisation"
                    : "ConnectedEntityCheckAnswersIndividualOrTrust")
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state, true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                    ? "ConnectedEntityCheckAnswersOrganisation"
                    : "ConnectedEntityCheckAnswersIndividualOrTrust")
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid) return Page();

        state.LegalForm = LegalForm;
        state.LawRegistered = LawRegistered;

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
            ? "ConnectedEntityCheckAnswersOrganisation"
            : "ConnectedEntityCheckAnswersIndividualOrTrust");

        var redirectPage = (RedirectToCheckYourAnswer == true ? checkAnswerPage : GetRedirectLinkPageName(state));

        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });
    }

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        BackPageLink = GetBackLinkPageName(state);
        LegalFormDisplayText = $"What is {state.OrganisationName}'s legal form?";
        ConnectedEntityType = state.ConnectedEntityType;
        if (reset)
        {
            LegalForm = state.LegalForm;
            LawRegistered = state.LawRegistered;
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

    private string GetBackLinkPageName(ConnectedEntityState state)
    {
        var backPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        //Check if postal is same as register and decide                       
                        if ((state.RegisteredAddress?.AreSameAddress(state.PostalAddress) ?? false) == true)
                        {
                            backPage = $"postal-address-same-as-registered/{ConnectedEntityId}";
                        }
                        else
                        {
                            backPage = $"{AddressType.Postal}-address/{(state.PostalAddress?.Country == Country.UnitedKingdom ? "uk" : "non-uk")}/{ConnectedEntityId}";
                        }
                        break;

                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        //What date was registered as a 'connected person'? (Date)
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return backPage;
    }

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = "ConnectedEntityCompanyQuestion";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = "ConnectedEntityCheckAnswersOrganisation";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return redirectPage;
    }
}