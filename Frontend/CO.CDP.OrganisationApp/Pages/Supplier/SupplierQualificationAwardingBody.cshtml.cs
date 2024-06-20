using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierQualificationAwardingBodyModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please enter person or awarding body.")]
    public string? PersonOrAwardingBody { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            if (getSupplierInfoTask.Result.CompletedQualification)
            {
                // var hasPersonOrAwardingBody = getSupplierInfoTask.Result..FirstOrDefault(i => i.CountryName == "United Kingdom");
                // PersonOrAwardingBody = hasPersonOrAwardingBody?.PersonOrAwardingBody;

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
            var getOrganisationTask = organisationClient.GetOrganisationAsync(Id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(Id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            //await organisationClient.UpdateOrganisationAsync(Id,
            //  new UpdatedOrganisation
            //    (
            //        type: OrganisationUpdateType.Address,

            //        organisation: new OrganisationInfo(null,
            //            addresses: [
            //                new OrganisationAddress (
            //                streetAddress : AddressLine1,
            //                streetAddress2 : AddressLine2,
            //                postalCode : Postcode,
            //                locality: TownOrCity,
            //                countryName: Country,
            //                type: AddressType.Registered,
            //                region: Region
            //                )]
            //    )));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}
