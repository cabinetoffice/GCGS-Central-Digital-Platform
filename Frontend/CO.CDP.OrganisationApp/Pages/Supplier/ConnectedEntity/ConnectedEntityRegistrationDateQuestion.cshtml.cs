using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using System.Globalization;
using CO.CDP.Mvc.Validation;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityRegistrationDateQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_HasRegistrationDateError))]
    public bool? HasRegistrationDate { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistrationDate), true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_DayRequiredError))]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_DayInvalidError))]
    public string? Day { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistrationDate), true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_MonthRequiredError))]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_MonthInvalidError))]
    public string? Month { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistrationDate), true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_YearRequiredError))]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_YearInvalidError))]
    public string? Year { get; set; }

    [BindProperty]
    public string? RegistrationDate { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? Hint { get; set; }
    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public string? BackPageLink { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }

    public IActionResult OnGet(bool? selected)
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

        InitModel(state, true);
        HasRegistrationDate = selected.HasValue ? selected : state.HasRegistrationDate;

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

        InitModel(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (state.HasRegistrationDate != HasRegistrationDate)
        { RedirectToCheckYourAnswer = false; }

        if (HasRegistrationDate == true)
        {
            var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                ModelState.AddModelError(nameof(RegistrationDate), StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_DateInvalidError);
                return Page();
            }

            state.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));
        }
        else
        { state.RegistrationDate = null; }
        
        state.HasRegistrationDate = HasRegistrationDate;
        if (HasRegistrationDate == false)
        {
            state.RegisterName = null;
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
                        redirectPage = HasRegistrationDate == true
                                        ? "ConnectedEntityCompanyRegisterName"
                                        : "ConnectedEntityCheckAnswersOrganisation";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = HasRegistrationDate == true
                                        ? "ConnectedEntityCompanyRegisterName"
                                        : "ConnectedEntityLegalFormQuestion";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = HasRegistrationDate == true
                                        ? "ConnectedEntityCompanyRegisterName"
                                        : "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual:
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        redirectPage = HasRegistrationDate == true
                                        ? "ConnectedEntityCompanyRegisterName"
                                        : "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust:
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                }
                break;
        }

        return redirectPage;
    }

    private void InitModel(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = string.Format(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRegistrationDateQuestion_Heading, (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation ? state.OrganisationName : state.FirstName));
        BackPageLink = GetBackLinkPageName(state);
        ConnectedEntityType = state.ConnectedEntityType;
        if (reset && state.RegistrationDate.HasValue)
        {
            Day = state.RegistrationDate.Value.Day.ToString();
            Month = state.RegistrationDate.Value.Month.ToString();
            Year = state.RegistrationDate.Value.Year.ToString();
        }
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
                        backPage = state.HasCompaniesHouseNumber == true ? "company-question" : "nature-of-control";
                        break;

                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        backPage = "nature-of-control";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        backPage = "nature-of-control";
                        break;
                }
                break;
        }

        return backPage;
    }
}
