using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<RegisterOrganisation, Model.Organisation>
{
    public RegisterOrganisationUseCase(IOrganisationRepository organisationRepository,
        IPersonRepository personRepository, IMapper mapper)
        : this(organisationRepository, personRepository, mapper, Guid.NewGuid)
    {
    }

    public async Task<Model.Organisation> Execute(RegisterOrganisation command)
    {
        var person = await FindPerson(command);
        var organisation = CreateOrganisation(command, person);
        organisationRepository.Save(organisation);
        return mapper.Map<Model.Organisation>(organisation);
    }

    private async Task<Person> FindPerson(RegisterOrganisation command)
    {
        Person? person = await personRepository.Find(command.PersonId);
        if (person == null)
        {
            throw new RegisterOrganisationException.UnknownPersonException($"Unknown person {command.PersonId}.");
        }

        return person;
    }

    private OrganisationInformation.Persistence.Organisation CreateOrganisation(
        RegisterOrganisation command,
        Person person
    )
    {
        var organisation = MapRequestToOrganisation(command, person);
        organisation.Persons.Add(person);
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

    public abstract class RegisterOrganisationException(string message, Exception? cause = null)
        : Exception(message, cause)
    {
        public class UnknownPersonException(string message, Exception? cause = null) : Exception(message, cause);
    }
}