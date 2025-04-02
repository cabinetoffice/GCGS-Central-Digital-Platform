namespace CO.CDP.OrganisationInformation.Persistence;

public interface IMouRepository : IDisposable
{
    Task<IEnumerable<MouReminderOrganisation>> GetMouReminderOrganisations();

    Task UpsertMouEmailReminder(int organisationId);
}
