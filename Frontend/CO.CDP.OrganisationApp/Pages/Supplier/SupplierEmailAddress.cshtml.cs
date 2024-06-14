using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierEmailAddressModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

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

            if (getSupplierInfoTask.Result.CompletedEmailAddress)
            {
                if (organisation.ContactPoint.Email != null)
                {
                    EmailAddress = organisation.ContactPoint.Email;
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
                    email: EmailAddress,
                    telephone: organisation.ContactPoint.Telephone,
                    url: organisation.ContactPoint.Url?.ToString());

            await organisationClient.UpdateOrganisationAsync(Id,
                new UpdatedOrganisation
                (
                    type: OrganisationUpdateType.ContactPoint,
                    organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: cp)
                ));

            await organisationClient.UpdateSupplierInformationAsync(Id,
                new UpdateSupplierInformation
                (
                    type: SupplierInformationUpdateType.CompletedEmailAddress,
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