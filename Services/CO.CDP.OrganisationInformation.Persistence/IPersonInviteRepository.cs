namespace CO.CDP.OrganisationInformation.Persistence;

public interface IPersonInviteRepository : IDisposable
{
    Task<PersonInvite?> Find(Guid personInviteId);

    Task<bool> Delete(PersonInvite personInviteId);

    Task<IEnumerable<PersonInvite>> FindByOrganisation(Guid organisationId);

    Task<bool> IsInviteEmailUniqueWithinOrganisation(Guid organisationId, string email);

    Task<IEnumerable<PersonInvite>> FindPersonInviteByEmail(Guid organisationId, string email);

    void Save(PersonInvite personInvite);
}