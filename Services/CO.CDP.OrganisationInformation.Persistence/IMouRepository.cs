using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IMouRepository : IDisposable
{
    Task<IEnumerable<MouReminderOrganisation>> GetMouReminderOrganisations(int daysBetweenReminders);

    Task UpsertMouEmailReminder(int organisationId);
}
