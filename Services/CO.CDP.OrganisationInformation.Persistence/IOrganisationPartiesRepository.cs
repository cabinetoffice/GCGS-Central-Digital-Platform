namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationPartiesRepository : IDisposable
{
    public Task<IEnumerable<OrganisationParty>> Find(Guid organisationId);

    Task Save(OrganisationParty organisationParty);
}