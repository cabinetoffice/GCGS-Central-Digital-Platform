using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationNameModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("Enter your organisation's name")]
    [Required(ErrorMessage = "Enter your organisation's name")]
    public string? OrganisationName { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            OrganisationName = organisation.Name;

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
            await organisationClient.UpdateOrganisationName(Id, organisationName: OrganisationName!);
        }
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState); 
            return Page();
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }        
        return RedirectToPage("OrganisationOverview", new { Id });
    }

   
}
