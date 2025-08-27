using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class SupplierInformationSummaryModel(
    IOrganisationClient organisationClient,
    IFormsClient formsClient,
    IDataSharingClient dataSharingClient,
    IFeatureManager featureManager,
    ILogger<SupplierInformationSummaryModel> logger) : PageModel
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
        CDP.Organisation.WebApiClient.SupplierInformation? supplierInfo;
        try
        {
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);
            var getConnectedEntitiesTask = organisationClient.GetConnectedEntitiesAsync(id);
            var formSectionsTask = formsClient.GetFormSectionsAsync(new Guid(FormsEngine.OrganisationSupplierInfoFormId), id);

            await Task.WhenAll(getSupplierInfoTask, getConnectedEntitiesTask, formSectionsTask);

            supplierInfo = getSupplierInfoTask.Result;
            ConnectedEntities = getConnectedEntitiesTask.Result;

            var formSectionsResult = formSectionsTask.Result;
            FormSections = formSectionsResult.FormSections;

            logger.LogInformation("SupplierInformationSummary - OrganisationId: {OrganisationId}, FormId: {FormId}", id, FormsEngine.OrganisationSupplierInfoFormId);
            logger.LogInformation("SupplierInformationSummary - Raw FormSections count: {Count}", FormSections.Count);

            foreach (var section in FormSections)
            {
                logger.LogInformation("SupplierInformationSummary - Section: {SectionId}, Name: {SectionName}, Type: {Type} ({TypeValue}), AnswerSetCount: {AnswerSetCount}",
                    section.SectionId, section.SectionName, section.Type, (int)section.Type, section.AnswerSetCount);
            }

            var standardSections = FormSections.Where(s => s.Type == FormSectionType.Standard || s.Type == FormSectionType.Exclusions).ToList();
            var additionalSections = FormSections.Where(s => s.Type == FormSectionType.AdditionalSection).ToList();

            logger.LogInformation("SupplierInformationSummary - Standard/Exclusions sections count: {StandardCount}", standardSections.Count);
            logger.LogInformation("SupplierInformationSummary - Additional sections count: {AdditionalCount}", additionalSections.Count);

            foreach (var additionalSection in additionalSections)
            {
                logger.LogInformation("SupplierInformationSummary - Additional section: {SectionId}, Name: {SectionName}, Type: {Type} ({TypeValue})",
                    additionalSection.SectionId, additionalSection.SectionName, additionalSection.Type, (int)additionalSection.Type);
            }

            logger.LogInformation("SupplierInformationSummary - WebApiClient.FormSectionType.AdditionalSection enum value: {AdditionalSectionEnumValue}", (int)CDP.Forms.WebApiClient.FormSectionType.AdditionalSection);

            var isAdditionalSectionEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SupplierAdditionalModule);
            logger.LogInformation("SupplierInformationSummary - SupplierAdditionalModule feature flag enabled: {FeatureFlagEnabled}", isAdditionalSectionEnabled);

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

    public async Task<int> GetShareCodesCount()
    {
        return (await dataSharingClient.GetShareCodeListAsync(Id)).Count;
    }
}