using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationEmailModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("Enter the organisation's contact email address")]
    [Required(ErrorMessage = "Enter the organisation's contact email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            EmailAddress = organisation.ContactPoint.Email;

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var organisation = await organisationClient.GetOrganisationAsync(Id);

        if (organisation == null)
        {
            return Redirect("/page-not-found");
        }

        try
        {
            var cp = new OrganisationContactPoint(
                    name: organisation.ContactPoint.Name,
                    email: EmailAddress,
                    telephone: organisation.ContactPoint.Telephone,
                    url: organisation.ContactPoint.Url?.ToString());


            await organisationClient.UpdateOrganisationEmail(Id, cp);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }


        return RedirectToPage("OrganisationOverview", new { Id });
    }
}