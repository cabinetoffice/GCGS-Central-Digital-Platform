using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;

namespace CO.CDP.OrganisationApp;

public class ShareCodeMandatoryInformationService : IShareCodeMandatoryInformationService
{
    private readonly IOrganisationClient _organisationClient;
    private readonly IFormsClient _formsClient;

    public ShareCodeMandatoryInformationService(IOrganisationClient organisationClient, IFormsClient formsClient)
    {
        _organisationClient = organisationClient;
        _formsClient = formsClient;
    }

    public async Task<bool> MandatorySectionsCompleted(Guid organisationId)
    {
        try
        {
            var getSupplierInfoTask = _organisationClient.GetOrganisationSupplierInformationAsync(organisationId);
            var getConnectedEntitiesTask = _organisationClient.GetConnectedEntitiesAsync(organisationId);
            var formSectionsTask = _formsClient.GetFormSectionsAsync(new Guid(FormsEngine.OrganisationSupplierInfoFormId), organisationId);

            await Task.WhenAll(getSupplierInfoTask, getConnectedEntitiesTask, formSectionsTask);

            var supplierInfo = getSupplierInfoTask.Result;
            var connectedEntities = getConnectedEntitiesTask.Result;
            var formSections = formSectionsTask.Result.FormSections;

            return SupplierInformationStatus.GetBasicInfoStepStatus(supplierInfo) == SupplierInformationStatus.StepStatus.Completed
                && SupplierInformationStatus.GetConnectedPersonStepStatus(supplierInfo, connectedEntities.Count) == SupplierInformationStatus.StepStatus.Completed
                && formSections
                    .Where(s => s.Type != FormSectionType.Declaration
                                && s.Type != FormSectionType.AdditionalSection
                                && s.Type != FormSectionType.WelshAdditionalSection)
                    .All(section => (section.AnswerSetCount != 0 || section.AnswerSetWithFurtherQuestionExemptedExists));
        }
        catch (Exception ex)
            when ((ex is CO.CDP.Organisation.WebApiClient.ApiException oex && oex.StatusCode == 404)
                || (ex is CDP.Forms.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return false;
        }
    }
}
