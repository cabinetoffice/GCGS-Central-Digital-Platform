namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationRepository : IDisposable
{
    public void Save(Organisation organisation);
    public Task SaveAsync(Organisation organisation, Func<Organisation, Task> onSave);

    public void SaveOrganisationPerson(OrganisationPerson organisationPerson);

    public Task<Organisation?> Find(Guid organisationId);
    public Task<Organisation?> FindIncludingPersons(Guid organisationId);
    public Task<Organisation?> FindIncludingTenant(Guid organisationId);
    public Task<Organisation?> FindIncludingTenantByOrgId(int id);
    public Task<IEnumerable<OrganisationPerson>> FindOrganisationPersons(Guid organisationId);

    public Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, Guid personId);

    public Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, string userUrn);

    public Task<Organisation?> FindByName(string name);

    public Task<IEnumerable<Organisation>> FindByUserUrn(string userUrn);

    public Task<Organisation?> FindByIdentifier(string scheme, string identifierId);

    public Task<IList<Organisation>> Get(string? type);

    public class OrganisationRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateOrganisationException(string message, Exception? cause = null)
            : OrganisationRepositoryException(message, cause);
    }

    public Task<IList<ConnectedEntity>> GetConnectedIndividualTrusts(int organisationId);

    public Task<IList<ConnectedEntity>> GetConnectedOrganisations(int organisationId);

    public Task<IList<ConnectedEntity>> GetConnectedTrustsOrTrustees(int organisationId);

    public Task<Organisation.LegalForm?> GetLegalForm(int organisationId);

    public Task<IList<OperationType>> GetOperationTypes(int organisationId);

    public Task<bool> IsEmailUniqueWithinOrganisation(Guid organisationId, string email);
    Task<Organisation?> FindIncludingReviewedBy(Guid organisationId);
}