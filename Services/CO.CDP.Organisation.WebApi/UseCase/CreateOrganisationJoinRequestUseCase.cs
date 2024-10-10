using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using OrganisationJoinRequest = CO.CDP.Organisation.WebApi.Model.OrganisationJoinRequest;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;
using AutoMapper;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class CreateOrganisationJoinRequestUseCase(
    Persistence.IOrganisationRepository organisationRepository,
    Persistence.IPersonRepository personRepository,
    Persistence.IOrganisationJoinRequestRepository organisationJoinRequestRepository,
    IConfiguration configuration,
    Func<Guid> guidFactory,
    IMapper mapper)
    : IUseCase<(Guid organisationId, CreateOrganisationJoinRequest createOrganisationJoinRequestCommand), OrganisationJoinRequest>
{
    public CreateOrganisationJoinRequestUseCase(
        Persistence.IOrganisationRepository organisationRepository,
        Persistence.IPersonRepository personRepository,
        Persistence.IOrganisationJoinRequestRepository organisationJoinRequestRepository,
        IConfiguration configuration,
        IMapper mapper
    ) : this(organisationRepository, personRepository, organisationJoinRequestRepository, configuration, Guid.NewGuid, mapper)
    {

    }

    public async Task<OrganisationJoinRequest> Execute((Guid organisationId, CreateOrganisationJoinRequest createOrganisationJoinRequestCommand) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var person = await personRepository.Find(command.createOrganisationJoinRequestCommand.PersonId)
                           ?? throw new UnknownPersonException($"Unknown person {command.createOrganisationJoinRequestCommand.PersonId}.");

        var organisationJoinRequest = CreateOrganisationJoinRequest(organisation, person);

        organisationJoinRequestRepository.Save(organisationJoinRequest);

        return mapper.Map<OrganisationJoinRequest>(organisationJoinRequest);
    }

    private Persistence.OrganisationJoinRequest CreateOrganisationJoinRequest(
        Persistence.Organisation organisation,
        Person person
    )
    {
        var organisationJoinRequest = new Persistence.OrganisationJoinRequest
        {
            Guid = guidFactory(),
            Organisation = organisation,
            Person = person,
            Status = OrganisationJoinRequestStatus.Pending
        };

        return organisationJoinRequest;
    }
}