using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class TradeAssuranceAwardedDateModel(
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a day")]
    [RegularExpression(@"^(0?[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a month")]
    [RegularExpression(@"^(0?[1-9]|1[0-2])$", ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a year")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? DateOfAward { get; set; }

    public IActionResult OnGet()
    {
        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        if (ta.DateAwarded.HasValue)
        {
            Day = ta.DateAwarded.Value.Day.ToString();
            Month = ta.DateAwarded.Value.Month.ToString();
            Year = ta.DateAwarded.Value.Year.ToString();
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            ModelState.AddModelError(nameof(DateOfAward), "Date of award must be a real date");
            return Page();
        }

        if (parsedDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(DateOfAward), "Date of award must be today or in the past");
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        ta.DateAwarded = parsedDate;
        tempDataService.Put(TradeAssurance.TempDataKey, ta);

        return RedirectToPage("TradeAssuranceCheckAnswer", new { Id });
    }
}