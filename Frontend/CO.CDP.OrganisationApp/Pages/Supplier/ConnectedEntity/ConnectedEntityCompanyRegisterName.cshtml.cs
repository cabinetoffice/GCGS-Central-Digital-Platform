using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCompanyRegisterNameModel(ISession session) : PageModel
{
    private const string OPTION_OTHER = "Other";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select if organisation is registered as person with significant control")]
    public string? RegisterName { get; set; }

    [BindProperty]
    [DisplayName("Other register name")]
    [RequiredIf("RegisterName", "Other")]
    public string? RegisterNameInput { get; set; }

    [BindProperty]
    public bool SupplierHasCompanyHouseNumber { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? BackPageLink { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state, true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid) return Page();

        state.RegisterName = RegisterName != OPTION_OTHER ? RegisterName : RegisterNameInput;

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
        Heading = $"Select where {state.OrganisationName} is registered as person with significant control";
        ConnectedEntityType = state.ConnectedEntityType;
        SupplierHasCompanyHouseNumber = state.SupplierHasCompanyHouseNumber ?? false;
        BackPageLink = GetBackLinkPageName(state);

        if (reset)
        {
            RegisterName = state.RegisterName;

            if (!string.IsNullOrEmpty(RegisterName) && !RegisterNameType.ContainsKey(RegisterName))
            {
                RegisterName = OPTION_OTHER;
                RegisterNameInput = state.RegisterName;
            }
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

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                        redirectPage = "ConnectedEntityCheckAnswersOrganisation";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = state.SupplierHasCompanyHouseNumber == false
                                        ? "ConnectedEntityLegalFormQuestion"
                                        : "";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        redirectPage = "ConnectedEntityCheckAnswersIndividualOrTrust";
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
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "date-registered"
                                    : "date-registered-question";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        backPage = state.SupplierHasCompanyHouseNumber == false
                                    ? "date-registered-question"
                                    : "";
                        break;

                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "date-registered"
                                    : "date-registered-question";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "date-registered"
                                    : "date-registered-question";
                        break;
                }
                break;
        }

        return backPage;
    }

    public static Dictionary<string, string> RegisterNameType => new()
    {
        { "CompaniesHouse", "People with significant control register on Companies House" }
    };
}