using AutoMapper;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.MQ;

public class MqOrganisationRepository(
    IOrganisationRepository organisationRepository,
    IPublisher publisher,
    IMapper mapper)
    : IOrganisationRepository
{
    public void Save(OrganisationInformation.Persistence.Organisation organisation)
    {
        var organisationId = organisation.Id;
        organisationRepository.Save(organisation);
        if (organisationId < 1)
        {
            publisher.Publish(mapper.Map<OrganisationRegistered>(organisation));
        }
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