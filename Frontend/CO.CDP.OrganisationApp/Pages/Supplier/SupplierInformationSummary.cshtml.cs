using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class SupplierInformationSummaryModel(
    IOrganisationClient organisationClient,
    IFormsClient formsClient) : PageModel
{
    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public SupplierInformationStatus.StepStatus BasicInformationStepStatus { get; set; }

    [BindProperty]
    public SupplierInformationStatus.StepStatus ConnectedPersonStepStatus { get; set; }

    [BindProperty]
    public ICollection<ConnectedEntityLookup> ConnectedEntities { get; set; } = [];

    public ICollection<FormSectionSummary> FormSections { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public bool HasSupplierType { get; set; }

    public Guid FormId { get; set; }
    public Guid SectionId { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        SupplierInformation? supplierInfo;
        try
        {
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);
            var getConnectedEntitiesTask = organisationClient.GetConnectedEntitiesAsync(id);
            var formSectionsTask = formsClient.GetFormSectionsAsync(new Guid(FormsEngine.OrganisationSupplierInfoFormId), id);

            await Task.WhenAll(getSupplierInfoTask, getConnectedEntitiesTask, formSectionsTask);

            supplierInfo = getSupplierInfoTask.Result;
            ConnectedEntities = getConnectedEntitiesTask.Result;
            FormSections = formSectionsTask.Result.FormSections;

            HasSupplierType = supplierInfo.SupplierType.HasValue;
        }
        catch (Exception ex)
            when ((ex is CO.CDP.Organisation.WebApiClient.ApiException oex && oex.StatusCode == 404)
                || (ex is CDP.Forms.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return Redirect("/page-not-found");
        }

        Name = supplierInfo.OrganisationName;
        BasicInformationStepStatus = SupplierInformationStatus.GetBasicInfoStepStatus(supplierInfo);
        ConnectedPersonStepStatus = SupplierInformationStatus.GetConnectedPersonStepStatus(supplierInfo, ConnectedEntities.Count);
        return Page();
    }
}