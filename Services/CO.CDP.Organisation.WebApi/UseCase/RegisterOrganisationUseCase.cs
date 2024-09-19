using AutoMapper;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository,
    IPublisher publisher,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<RegisterOrganisation, Model.Organisation>
{
    private readonly List<string> _defaultScopes = ["ADMIN", "RESPONDER"];

    public RegisterOrganisationUseCase(IOrganisationRepository organisationRepository,
        IPersonRepository personRepository, IPublisher publisher, IMapper mapper)
        : this(organisationRepository, personRepository, publisher, mapper, Guid.NewGuid)
    {
    }

    public async Task<Model.Organisation> Execute(RegisterOrganisation command)
    {
        var person = await FindPerson(command);
        var organisation = CreateOrganisation(command, person);
        organisationRepository.Save(organisation);
        await publisher.Publish(mapper.Map<OrganisationRegistered>(organisation));
        return mapper.Map<Model.Organisation>(organisation);
    }

    private async Task<Person> FindPerson(RegisterOrganisation command)
    {
        Person? person = await personRepository.Find(command.PersonId);
        if (person == null)
        {
            throw new UnknownPersonException($"Unknown person {command.PersonId}.");
        }

        return person;
    }

    private OrganisationInformation.Persistence.Organisation CreateOrganisation(
        RegisterOrganisation command,
        Person person
    )
    {
        var organisation = MapRequestToOrganisation(command, person);
        organisation.OrganisationPersons.Add(new OrganisationPerson
        {
            Person = person,
            Organisation = organisation,
            Scopes = _defaultScopes
        });
        organisation.UpdateBuyerInformation();
        organisation.UpdateSupplierInformation();
        return organisation;
    }

    private OrganisationInformation.Persistence.Organisation MapRequestToOrganisation(
        RegisterOrganisation command,
        Person person
    ) =>
        mapper.Map<OrganisationInformation.Persistence.Organisation>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
            o.Items["Tenant"] = new Tenant
            {
                Guid = guidFactory(),
                Name = command.Name,
                Persons = { person }
            };
        });
}