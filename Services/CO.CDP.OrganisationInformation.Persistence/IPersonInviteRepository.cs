namespace CO.CDP.OrganisationInformation.Persistence;

public interface IPersonInviteRepository : IDisposable
{
    Task<PersonInvite?> Find(Guid personInviteId);

    Task<bool> Delete(PersonInvite personInviteId);

    Task<IEnumerable<PersonInvite>> FindByOrganisation(Guid organisationId);

    void Save(PersonInvite personInvite);
}