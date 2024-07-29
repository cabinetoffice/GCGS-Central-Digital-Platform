using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityControlConditionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [NotEmpty(ErrorMessage = "Select the Which specified conditions of control does your organisation have?")]
    public required List<ConnectedEntityControlCondition> ControlConditions { get; set; } = [];

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public string? BackPageLink { get; set; }
    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? "ConnectedEntityCheckAnswersOrganisation"
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

        state.ControlConditions = ControlConditions;

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                    ? "ConnectedEntityCheckAnswersOrganisation"
                    : "ConnectedEntityCheckAnswersIndividualOrTrust");

        var redirectPage = (RedirectToCheckYourAnswer == true
                        ? checkAnswerPage
                        : GetRedirectLinkPageName(state));

        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });
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

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = $"Which specified conditions of control does {state.OrganisationName} have?";
        BackPageLink = GetBackLinkPageName(state);
        ConnectedEntityType = state.ConnectedEntityType;
        if (reset)
        {
            ControlConditions = state.ControlConditions;
        }
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
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                }
                break;
        }

        return redirectPage;
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
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        backPage = $"company-question/{ConnectedEntityId}";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        backPage = $"{AddressType.Registered}-address/{(state.RegisteredAddress?.Country == Country.UnitedKingdom ? "uk" : "non-uk")}/{ConnectedEntityId}";                        
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        backPage = "";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust:
                        backPage = "";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        backPage = "";
                        break;
                }
                break;
        }

        return backPage;
    }

}