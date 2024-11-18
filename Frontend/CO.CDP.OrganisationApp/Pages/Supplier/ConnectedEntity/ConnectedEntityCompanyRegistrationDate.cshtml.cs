using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityCompanyRegistrationDateModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    public ConnectedEntityType? ConnectedEntityType { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_DayRequiredError))]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_DayInvalidError))]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_MonthRequiredError))]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_MonthInvalidError))]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_YearRequiredError))]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_YearInvalidError))]
    public string? Year { get; set; }

    [BindProperty]
    public string? RegistrationDate { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }

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

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            ModelState.AddModelError(nameof(RegistrationDate), StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_DateInvalidError);
            return Page();
        }

        state.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));

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
                        redirectPage = "ConnectedEntityCompanyRegisterName";
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
                        redirectPage = "ConnectedEntityLawRegister";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = "ConnectedEntityCompanyRegisterName";
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
                        redirectPage = "ConnectedEntityCompanyRegisterName";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityCheckAnswersIndividualOrTrust";
                        break;
                }
                break;
        }

        return redirectPage;
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
        Heading = string.Format(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyRegistrationDate_Heading,
            (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation ? state.OrganisationName : state.FirstName));

        ConnectedEntityType = state.ConnectedEntityType;

        if (reset && state.RegistrationDate.HasValue)
        {
            Day = state.RegistrationDate.Value.Day.ToString();
            Month = state.RegistrationDate.Value.Month.ToString();
            Year = state.RegistrationDate.Value.Year.ToString();
        }
    }
}
