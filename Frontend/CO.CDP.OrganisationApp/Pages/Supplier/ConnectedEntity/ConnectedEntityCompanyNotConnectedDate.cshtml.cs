using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityCompanyNotConnectedDateModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = "Global_DateInput_DayRequiredError", ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Day, ErrorMessageResourceName = "Global_DateInput_DayInvalidError", ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = "Global_DateInput_MonthRequiredError", ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Month, ErrorMessageResourceName = "Global_DateInput_MonthInvalidError", ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = "Global_DateInput_YearRequiredError", ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Year, ErrorMessageResourceName = "Global_DateInput_YearInvalidError", ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Year { get; set; }

    [BindProperty]
    public string? InsolvencyDate { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string? Heading { get; set; }

    public IActionResult OnGet()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);

        InitModel(state!, reset: true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);

        InitModel(state!);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            ModelState.AddModelError(nameof(InsolvencyDate), StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyInsolvencyDate_InvalidDate);
            return Page();
        }

        state!.EndDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
            ? "ConnectedEntityCheckAnswersOrganisation"
            : "ConnectedEntityCheckAnswersIndividualOrTrust");

        return RedirectToPage(checkAnswerPage, new { Id, ConnectedEntityId });
    }

    private void InitModel(ConnectedEntityState state, bool reset = false)
    {
        if ((state.ConnectedEntityType == ConnectedEntityType.Individual) || (state.ConnectedEntityType == ConnectedEntityType.TrustOrTrustee))
        {
            Heading = string.Format(StaticTextResource.Supplier_ConnectedEntity_NotConnectedDate_Heading, string.Join(' ', new[] { state.FirstName ?? "", state.LastName ?? "" }));
        }
        else
        {
            Heading = string.Format(StaticTextResource.Supplier_ConnectedEntity_NotConnectedDate_Heading, state.OrganisationName);
        }
        
        if (reset && state.EndDate.HasValue)
        {
            Day = state.EndDate.Value.Day.ToString();
            Month = state.EndDate.Value.Month.ToString();
            Year = state.EndDate.Value.Year.ToString();
        }
    }
}
