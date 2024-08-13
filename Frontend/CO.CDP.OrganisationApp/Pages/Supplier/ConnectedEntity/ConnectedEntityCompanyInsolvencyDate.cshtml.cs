using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCompanyInsolvencyDateModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of Insolvency must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of Insolvency must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of Insolvency must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? InsolvencyDate { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }
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

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            ModelState.AddModelError(nameof(InsolvencyDate), "Date of insolvency must be a real date");
            return Page();
        }

        if (parsedDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(InsolvencyDate), "Date of insolvency must be today or in the past");
            return Page();
        }

        state.InsolvencyDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
            ? "ConnectedEntityCheckAnswersOrganisation"
            : "ConnectedEntityCheckAnswersIndividualOrTrust");

        return RedirectToPage(checkAnswerPage, new { Id, ConnectedEntityId });
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
        Heading = $"When did {state.OrganisationName} become insolvent?";
        BackPageLink = GetBackLinkPageName(state);

        state.RegistrationDate = null;
        state.RegisterName = null;
        state.LawRegistered = null;
        if (reset && state.InsolvencyDate.HasValue)
        {
            Day = state.InsolvencyDate.Value.Day.ToString();
            Month = state.InsolvencyDate.Value.Month.ToString();
            Year = state.InsolvencyDate.Value.Year.ToString();
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
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "company-question"
                                    : (state.HasCompaniesHouseNumber == true
                                        ? "company-question"
                                        : "overseas-company-question");
                        break;
                }
                break;
        }

        return backPage;
    }
}