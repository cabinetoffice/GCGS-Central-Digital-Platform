using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityRegistrationDateQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you required to register organisation as a person with significant control")]
    public bool? HasRegistartionDate { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistartionDate), true, ErrorMessage = "Date of registration must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistartionDate), true, ErrorMessage = "Date of registration must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasRegistartionDate), true, ErrorMessage = "Date of registration must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? RegistrationDate { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? Hint { get; set; }
    [BindProperty]
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

        InitModal(state, true);

        HasRegistartionDate = selected.HasValue ? selected : state.HasRegistartionDate;

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

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (HasRegistartionDate == true)
        {
            var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                ModelState.AddModelError(nameof(RegistrationDate), "Date of registration must be a real date");
                return Page();
            }

            state.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));
        }
        else
        { state.RegistrationDate = null; }

        state.HasRegistartionDate = HasRegistartionDate;
        if (HasRegistartionDate == false)
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
                        redirectPage = HasRegistartionDate == true ? "ConnectedEntityCompanyRegisterName" : "ConnectedEntityCheckAnswersOrganisation";
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = "ConnectedEntityCheckAnswersOrganisation";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = "ConnectedEntityCompanyInsolvencyDate";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = state.HasCompaniesHouseNumber == true ? "ConnectedEntityControlCondition" : "ConnectedEntityOverseasCompanyQuestion";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = HasRegistartionDate == true
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
                        redirectPage = HasRegistartionDate == true
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

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = $"Were you required to register {state.OrganisationName} as a person with significant control?";
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