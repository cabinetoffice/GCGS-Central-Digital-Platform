using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class RegisterPersonUseCase(IPersonRepository personRepository, IMapper mapper, Func<Guid> guidFactory)
    : IUseCase<RegisterPerson, Model.Person>
{
    public RegisterPersonUseCase(IPersonRepository personRepository, IMapper mapper)
        : this(personRepository, mapper, Guid.NewGuid)
    {
    }

    public Task<Model.Person> Execute(RegisterPerson command)
    {
        var person =
            mapper.Map<OrganisationInformation.Persistence.Person>(command, o => o.Items["Guid"] = guidFactory());
        personRepository.Save(person);
        return Task.FromResult(mapper.Map<Model.Person>(person));
    }
}