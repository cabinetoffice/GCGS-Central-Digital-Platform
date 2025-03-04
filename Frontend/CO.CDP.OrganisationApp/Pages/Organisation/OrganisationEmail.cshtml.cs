using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationEmailModel(OrganisationWebApiClient.IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [ModelBinder<SanitisedStringModelBinder>]
    [DisplayName(nameof(StaticTextResource.Organisation_Email_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Organisation_Email_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
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
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (OrganisationWebApiClient.ApiException ex) when (ex.StatusCode == 404)
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
            var cp = new OrganisationWebApiClient.OrganisationContactPoint(
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
