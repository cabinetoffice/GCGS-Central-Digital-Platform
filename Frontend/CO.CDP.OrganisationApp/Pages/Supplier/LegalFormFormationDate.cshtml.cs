using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class LegalFormFormationDateModel(
    ITempDataService tempDataService,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

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

    public async Task<IActionResult> OnGet()
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var lf = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        if (lf.RegistrationDate.HasValue)
        {
            Day = lf.RegistrationDate.Value.Day.ToString();
            Month = lf.RegistrationDate.Value.Month.ToString();
            Year = lf.RegistrationDate.Value.Year.ToString();
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
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

        var lf = tempDataService.GetOrDefault<LegalForm>(LegalForm.TempDataKey);
        lf.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));
        tempDataService.Put(LegalForm.TempDataKey, lf);

        if (!Validate(lf))
        {
            return RedirectToPage("LegalFormCompanyActQuestion", new { Id });
        }

        var legalform = new CO.CDP.Organisation.WebApiClient.LegalForm
                        (
                            lf.LawRegistered,
                            lf.RegisteredLegalForm,
                            lf.RegisteredUnderAct2006!.Value,
                            lf.RegistrationDate!.Value
                        );
        try
        {
            await organisationClient.UpdateSupplierLegalForm(Id, legalform);

            tempDataService.Remove(TradeAssurance.TempDataKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }

    private static bool Validate(LegalForm legalForm)
    {
        return !string.IsNullOrEmpty(legalForm.LawRegistered)
            && !string.IsNullOrEmpty(legalForm.RegisteredLegalForm)
            && legalForm.RegistrationDate.HasValue;
    }
}