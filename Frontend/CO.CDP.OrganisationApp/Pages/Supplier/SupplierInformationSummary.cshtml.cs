using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierInformationSummaryModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public StepStatus BasicInformationStepStatus { get; set; }

    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public bool HasSupplierType { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        SupplierInformation? supplierInfo;
        try
        {
            supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(id);
            HasSupplierType = supplierInfo.SupplierType.HasValue;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        Name = supplierInfo.OrganisationName;
        BasicInformationStepStatus = GetBasicInfoStepStatus(supplierInfo);

        return Page();
    }

    private static StepStatus GetBasicInfoStepStatus(SupplierInformation info)
    {
        if (info.SupplierType == null) return StepStatus.NotStarted;

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
}

public enum StepStatus
{
    NotStarted,
    InProcess,
    Completed,
}