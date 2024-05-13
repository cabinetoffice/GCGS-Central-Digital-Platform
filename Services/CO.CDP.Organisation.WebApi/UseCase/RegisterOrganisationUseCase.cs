using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
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
        Person? person = await personRepository.Find(command.PersonId);
        if (person == null)
        {
            throw new RegisterOrganisationException.UnknownPersonException($"Unknown person {command.PersonId}.");
        }

        var organisation = mapper.Map<OrganisationInformation.Persistence.Organisation>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
            o.Items["Tenant"] = new Tenant
            {
                Guid = guidFactory(),
                Name = command.Name,
                Persons = { person }
            };
        });
        organisationRepository.Save(organisation);
        return mapper.Map<Model.Organisation>(organisation);
    }

    public class RegisterOrganisationException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class UnknownPersonException(string message, Exception? cause = null) : Exception(message, cause);
    }
}