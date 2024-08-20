using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierQualificationAwardedDateModel(
   ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date of award must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? DateOfAward { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public IActionResult OnGet()
    {
        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        if (qa.DateAwarded.HasValue)
        {
            Day = qa.DateAwarded.Value.Day.ToString();
            Month = qa.DateAwarded.Value.Month.ToString();
            Year = qa.DateAwarded.Value.Year.ToString();
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

        var qa = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        qa.DateAwarded = parsedDate;
        tempDataService.Put(Qualification.TempDataKey, qa);

        return RedirectToPage(
                RedirectToCheckYourAnswer == true ? "SupplierQualificationCheckAnswer" : "SupplierQualificationName",
                new { Id });
    }
}