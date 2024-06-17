using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class AddressTypeQuestionModel(
    ISession session, IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasUkPrincipalAddress { get; set; }

    [BindProperty]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            if (getSupplierInfoTask.Result.CompletedRegAddress)
            {
                var hasRegisteredAddress = getOrganisationTask.Result.Addresses.FirstOrDefault(i => i.Type == AddressType.Registered);
                HasUkPrincipalAddress = hasRegisteredAddress != null;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }


    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (HasUkPrincipalAddress == true)
            {
                return RedirectToPage("PrincipalOfficeAddressUk", new { Id });
            }
            else
            {
                return RedirectToPage("PrincipalOfficeAddressNonUk", new { Id });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("/page-not-found");
        }
    }
}
