using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public static class SupplierInformationStatus
{
    public static StepStatus GetBasicInfoStepStatus(SupplierInformation info)
    {
        if (info.SupplierType == null)
            return StepStatus.NotStarted;

        return info.SupplierType.Value switch
        {
            SupplierType.Organisation => info.CompletedRegAddress && info.CompletedPostalAddress
                            && info.CompletedVat && info.CompletedWebsiteAddress
                            && info.CompletedEmailAddress
                            && info.CompletedOperationType && info.CompletedLegalForm ? StepStatus.Completed : StepStatus.InProcess,

            SupplierType.Individual => info.CompletedRegAddress && info.CompletedPostalAddress
                            && info.CompletedVat && info.CompletedWebsiteAddress
                            && info.CompletedEmailAddress
                            ? StepStatus.Completed : StepStatus.InProcess,

            _ => StepStatus.NotStarted,
        };
    }

    public static StepStatus GetConnectedPersonStepStatus(SupplierInformation info, int entityCount)
    {
        if (info == null)
            return StepStatus.NotStarted;

        if (info.CompletedConnectedPerson == false && entityCount == 0)
            return StepStatus.NotStarted;

        return info.CompletedConnectedPerson != true ? StepStatus.NotStarted : StepStatus.Completed;
    }

    public enum StepStatus
    {
        NotStarted,
        InProcess,
        Completed,
    }
}
