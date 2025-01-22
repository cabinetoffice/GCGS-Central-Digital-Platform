using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationRepository : IDisposable
{
    public void Save(Organisation organisation);
    public Task SaveAsync(Organisation organisation, Func<Organisation, Task> onSave);

    public void SaveOrganisationPerson(OrganisationPerson organisationPerson);

    public void SaveOrganisationMou(MouSignature mouSignature);

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

    public Task<IList<Organisation>> GetPaginated(PartyRole role, int limit, int skip);

    public Task<int> GetTotalCount(PartyRole role);

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

    public Task<IEnumerable<MouSignature>> GetMouSignatures(int organisationId);

    public Task<MouSignature?> GetMouSignature(int organisationId, Guid mouSignatureId);

    public Task<Mou?> GetLatestMou();

    public Task<Mou?> GetMou(Guid mouId);
    Task<IEnumerable<Organisation>> SearchByName(string name, PartyRole? role, int? limit);
}