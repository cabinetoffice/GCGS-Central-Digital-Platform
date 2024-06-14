using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierWebsiteQuestionModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasWebsiteAddress { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasWebsiteAddress), true, ErrorMessage = "Please enter the Website address.")]
    [Url]
    public string? WebsiteAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);
            var organisation = getOrganisationTask.Result;

            if (getSupplierInfoTask.Result.CompletedWebsiteAddress)
            {
                HasWebsiteAddress = false;
                if (organisation.ContactPoint.Url != null)
                {
                    HasWebsiteAddress = true;
                    WebsiteAddress = organisation.ContactPoint.Url.ToString();
                }
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);

            var cp = new OrganisationContactPoint(
                    name: organisation.ContactPoint.Name,
                    email: organisation.ContactPoint.Email,
                    telephone: organisation.ContactPoint.Telephone,
                    url: HasWebsiteAddress == true ? WebsiteAddress : null);

            await organisationClient.UpdateOrganisationAsync(Id,
                new UpdatedOrganisation
                (
                    type: OrganisationUpdateType.ContactPoint,
                    organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: cp)
                ));

            await organisationClient.UpdateSupplierInformationAsync(Id,
                new UpdateSupplierInformation
                (
                    type: SupplierInformationUpdateType.CompletedWebsiteAddress,
                    supplierInformation: new SupplierInfo(supplierType: null))
                );
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}