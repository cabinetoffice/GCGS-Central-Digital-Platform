namespace CO.CDP.OrganisationApp;

public interface IShareCodeMandatoryInformationService
{
    Task<bool> MandatorySectionsCompleted(Guid organisationId);
}
