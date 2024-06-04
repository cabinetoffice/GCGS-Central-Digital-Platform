using CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CompanyHouseApi.Integration.Pages;
public class IndexModel(ICompaniesHouseService companiesHouseService) : PageModel
{
    [BindProperty]
    [DisplayName("Enter Comapnies House Number")]
    [Required(ErrorMessage = "Enter Comapnies House Number")]
    public string? CompaniesHouseNumber { get; set; }
    public CompanyHouseDetails? CompanyDetails { get; set; }
    public List<Officer>? CompanyOfficers { get; set; }
    public List<PersonWithSignificantControl>? PersonsWithSignificantControl { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {

    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(CompaniesHouseNumber) || CompaniesHouseNumber.Length != 8)
        {
            ErrorMessage = "Company number must be 8 characters long.";
            return Page();
        }

        try
        {
            CompanyDetails = await companiesHouseService.GetCompanyAsync(CompaniesHouseNumber);
            CompanyOfficers = await companiesHouseService.GetCompanyOfficersListAsync(CompaniesHouseNumber);
            PersonsWithSignificantControl = await companiesHouseService.GetPersonsWithSignificantControlAsync(CompaniesHouseNumber);

            if (PersonsWithSignificantControl == null || !PersonsWithSignificantControl.Any())
            {
                ErrorMessage = "There are no persons with significant control in this company.";
            }

        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error fetching company details: {ex.Message}";
        }

        return Page();
    }
}
