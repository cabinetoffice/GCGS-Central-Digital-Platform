using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityCompanyQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if an organisation is registered with Companies House")]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = "Enter the Companies House number.")]
    public string? CompaniesHouseNumber { get; set; }
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

        InitModal(state);

        HasCompaniesHouseNumber = selected.HasValue ? selected : state.HasCompaniesHouseNumber;
        CompaniesHouseNumber = state.CompaniesHouseNumber;

        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (state.HasCompaniesHouseNumber != HasCompaniesHouseNumber)
        {
            RedirectToCheckYourAnswer = false;
        }

        state.HasCompaniesHouseNumber = HasCompaniesHouseNumber;
        state.CompaniesHouseNumber = CompaniesHouseNumber;
        if (HasCompaniesHouseNumber == true)
        {
            state.OverseasCompaniesHouseNumber = null;
            state.HasOverseasCompaniesHouseNumber = null;
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
                        redirectPage = (state.SupplierHasCompanyHouseNumber == true
                                            ? "ConnectedEntityControlCondition"
                                            : (state.HasCompaniesHouseNumber == true
                                                    ? "ConnectedEntityControlCondition"
                                                    : "ConnectedEntityOverseasCompanyQuestion"));
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = (state.SupplierHasCompanyHouseNumber == true
                                            ? "ConnectedEntityCheckAnswersOrganisation"
                                            : (state.HasCompaniesHouseNumber == true
                                                    ? "ConnectedEntityCheckAnswersOrganisation"
                                                    : "ConnectedEntityOverseasCompanyQuestion"));
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = (state.SupplierHasCompanyHouseNumber == true
                                            ? "ConnectedEntityCheckAnswersOrganisation"
                                            : (state.HasCompaniesHouseNumber == true
                                                    ? "ConnectedEntityCheckAnswersOrganisation"
                                                    : "ConnectedEntityOverseasCompanyQuestion"));
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = (state.SupplierHasCompanyHouseNumber == true
                                            ? "ConnectedEntityCompanyInsolvencyDate"
                                            : (state.HasCompaniesHouseNumber == true
                                                    ? "ConnectedEntityCompanyInsolvencyDate"
                                                    : "ConnectedEntityOverseasCompanyQuestion"));
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = state.HasCompaniesHouseNumber == true ? "ConnectedEntityControlCondition" : "ConnectedEntityOverseasCompanyQuestion";
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

    private void InitModal(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = $"Is {state.OrganisationName} registered with Companies House?";
        Hint = "Is the ‘connected person’ registered with Companies House as required by the Companies Act 2006.";
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
                        backPage = (state.SupplierHasCompanyHouseNumber == true
                                    ? "law-register" :
                                    (state.HasLegalForm == true ? "law-enforces" : "legal-form-question"));
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        //Check if postal is same as register and decide                       
                        if ((state.RegisteredAddress?.AreSameAddress(state.PostalAddress) ?? false) == true)
                        {
                            backPage = $"postal-address-same-as-registered";
                        }
                        else
                        {
                            backPage = $"{AddressType.Postal}-address/{(string.Equals(state.PostalAddress?.Country, Country.UKCountryCode, StringComparison.OrdinalIgnoreCase) ? "uk" : "non-uk")}";
                        }
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        backPage = $"{AddressType.Registered}-address/{(string.Equals(state.RegisteredAddress?.Country, Country.UKCountryCode, StringComparison.OrdinalIgnoreCase) ? "uk" : "non-uk")}";
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