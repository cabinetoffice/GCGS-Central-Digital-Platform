using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.MQ;

public class MqOrganisationRepository(IOrganisationRepository organisationRepository)
    : IOrganisationRepository
{
    public void Save(OrganisationInformation.Persistence.Organisation organisation)
    {
        organisationRepository.Save(organisation);
    }

    public Task<OrganisationInformation.Persistence.Organisation?> Find(Guid organisationId)
    {
        return organisationRepository.Find(organisationId);
    }

    public Task<OrganisationInformation.Persistence.Organisation?> FindByName(string name)
    {
        return organisationRepository.FindByName(name);
    }

    public Task<IEnumerable<OrganisationInformation.Persistence.Organisation>> FindByUserUrn(string userUrn)
    {
        return organisationRepository.FindByUserUrn(userUrn);
    }

    public Task<OrganisationInformation.Persistence.Organisation?> FindByIdentifier(string scheme, string identifierId)
    {
        return organisationRepository.FindByIdentifier(scheme, identifierId);
    }

    public void Dispose()
    {
        organisationRepository.Dispose();
    }
}