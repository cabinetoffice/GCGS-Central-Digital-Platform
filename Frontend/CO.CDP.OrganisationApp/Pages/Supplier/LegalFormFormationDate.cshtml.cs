using CO.CDP.Localization;
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
    [Required(ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_DayErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Day, ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_DayInvalidErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Day { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_MonthErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Month, ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_MonthInvalidErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Month { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_YearErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Year, ErrorMessageResourceName = nameof(@StaticTextResource.Supplier_LegalFormFormationDate_YearInvalidErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Year { get; set; }

    public bool? RegisteredUnderAct2006 { get; set; }

    public string? RegisteredLegalForm { get; set; }

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

        RegisteredUnderAct2006 = lf.RegisteredUnderAct2006;
        RegisteredLegalForm = lf.RegisteredLegalForm;

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
            ModelState.AddModelError(nameof(RegistrationDate), StaticTextResource.Supplier_LegalFormFormationDate_InvalidDateErrorMessage);
            return Page();
        }

        if (parsedDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(RegistrationDate), StaticTextResource.Supplier_LegalFormFormationDate_FutureDateErrorMessage);
            return Page();
        }

        var lf = tempDataService.GetOrDefault<LegalForm>(LegalForm.TempDataKey);
        lf.RegistrationDate = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));
        tempDataService.Put(LegalForm.TempDataKey, lf);

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

            tempDataService.Remove(LegalForm.TempDataKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}