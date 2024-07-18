using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCompanyRegistrationDateModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of registration must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of registration must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of registration must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? RegistrationDate { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
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
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            ModelState.AddModelError(nameof(RegistrationDate), "Date of registration must be a real date");
            return Page();
        }

        if (parsedDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(RegistrationDate), "Date of registration must be today or in the past");
            return Page();
        }

        state.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));

        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage("ConnectedEntityCompanyRegisterName", new { Id });
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
        Heading = $"What date was {state.OrganisationName} registered as a 'connected person'?";

        if (reset && state.RegistrationDate.HasValue)
        {
            Day = state.RegistrationDate.Value.Day.ToString();
            Month = state.RegistrationDate.Value.Month.ToString();
            Year = state.RegistrationDate.Value.Year.ToString();
        }
    }
}