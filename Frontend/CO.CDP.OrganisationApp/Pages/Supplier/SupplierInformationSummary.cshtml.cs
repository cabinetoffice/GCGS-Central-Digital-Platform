using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierInformationSummaryModel(
    IOrganisationClient organisationClient,
    IFormsClient formsClient) : PageModel
{
    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public StepStatus BasicInformationStepStatus { get; set; }

    [BindProperty]
    public StepStatus ConnectedPersonStepStatus { get; set; }

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
            when ((ex is Organisation.WebApiClient.ApiException oex && oex.StatusCode == 404)
                || (ex is CDP.Forms.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return Redirect("/page-not-found");
        }

        Name = supplierInfo.OrganisationName;
        BasicInformationStepStatus = GetBasicInfoStepStatus(supplierInfo);
        ConnectedPersonStepStatus = GetConnectedPersonStepStatus(supplierInfo, ConnectedEntities.Count);
        return Page();
    }

    private static StepStatus GetBasicInfoStepStatus(SupplierInformation info)
    {
        if (info.SupplierType == null)
            return StepStatus.NotStarted;

        return info.SupplierType.Value switch
        {
            SupplierType.Organisation => info.CompletedRegAddress && info.CompletedPostalAddress
                            && info.CompletedVat && info.CompletedWebsiteAddress
                            && info.CompletedEmailAddress && info.CompletedQualification
                            && info.CompletedTradeAssurance && info.CompletedOperationType && info.CompletedLegalForm ? StepStatus.Completed : StepStatus.InProcess,

            SupplierType.Individual => info.CompletedRegAddress && info.CompletedPostalAddress
                            && info.CompletedVat && info.CompletedWebsiteAddress
                            && info.CompletedEmailAddress && info.CompletedQualification
                            && info.CompletedTradeAssurance ? StepStatus.Completed : StepStatus.InProcess,

            _ => StepStatus.NotStarted,
        };
    }

    private static StepStatus GetConnectedPersonStepStatus(SupplierInformation info, int entityCount)
    {
        if (info == null)
            return StepStatus.NotStarted;

        if (info.CompletedConnectedPerson == false && entityCount == 0)
            return StepStatus.NotStarted;

        return info.CompletedConnectedPerson != true ? StepStatus.NotStarted : StepStatus.Completed;
    }
}

public enum StepStatus
{
    NotStarted,
    InProcess,
    Completed,
}