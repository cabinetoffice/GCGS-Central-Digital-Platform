using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityLegalFormQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_SelectYesOrNo), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? HasLegalForm { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasLegalForm), true, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityLegalFormQuestion_LegalFormNameRequiredError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? LegalFormName { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? Hint { get; set; }
    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public string? BackPageLink { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModel(state);

        HasLegalForm = selected ?? state.HasLegalForm;
        if (RedirectToCheckYourAnswer == true && HasLegalForm == null)
        {
            HasLegalForm = false;
        }
        LegalFormName = state.LegalForm;

        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModel(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (state.HasLegalForm != HasLegalForm)
        {
            RedirectToCheckYourAnswer = false;
        }

        state.HasLegalForm = HasLegalForm;
        state.LegalForm = (HasLegalForm == true ? LegalFormName : string.Empty);
        if (state.HasLegalForm == false)
        {
            state.LegalForm = null;
            state.LawRegistered = null;
        }

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
            ? "ConnectedEntityCheckAnswersOrganisation"
            : "ConnectedEntityCheckAnswersIndividualOrTrust");

        var redirectPage = (RedirectToCheckYourAnswer == true
                        ? checkAnswerPage
                        : GetRedirectLinkPageName(state));
        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });
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
                        redirectPage = (state.HasLegalForm == true ? "ConnectedEntityLawEnforce" : "ConnectedEntityCompanyQuestion");
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = state.SupplierHasCompanyHouseNumber == true
                            ? "ConnectedEntityCheckAnswersOrganisation"
                            : (state.HasLegalForm == true
                                ? "ConnectedEntityLawEnforce"
                                : "ConnectedEntityCompanyQuestion");
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = (state.HasLegalForm == true ? "ConnectedEntityLawEnforce" : "ConnectedEntityCheckAnswersOrganisation");
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

    private void InitModel(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = string.Format(
            StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityLegalFormQuestion_Heading,
            state.OrganisationName
        );
        Hint = string.Format(
            StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityLegalFormQuestion_Hint,
            state.OrganisationName
        );
        BackPageLink = GetBackLinkPageName(state);
    }

    private string GetBackLinkPageName(ConnectedEntityState state)
    {
        var backPage = "";
        switch (state.ConnectedEntityType)
        {
            case ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        if ((state.RegisteredAddress?.AreSameAddress(state.PostalAddress) ?? false) == true)
                        {
                            backPage = $"postal-address-same-as-registered";
                        }
                        else
                        {
                            backPage = $"{AddressType.Postal}-address/{(string.Equals(state.PostalAddress?.Country, Country.UKCountryCode, StringComparison.OrdinalIgnoreCase) ? "uk" : "non-uk")}";
                        }
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        backPage = state.RegistrationDate.HasValue
                                    ? "company-register-name"
                                    : "date-registered-question";
                        break;

                }
                break;
            case ConnectedEntityType.Individual:
                break;
            case ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return backPage;
    }
}