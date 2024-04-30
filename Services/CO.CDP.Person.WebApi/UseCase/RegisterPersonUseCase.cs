using AutoMapper;
using CO.CDP.Person.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class RegisterPersonUseCase(IPersonRepository PersonRepository, IMapper mapper, Func<Guid> guidFactory)
    : IUseCase<RegisterPerson, Model.Person>
{
    public RegisterPersonUseCase(IPersonRepository PersonRepository, IMapper mapper)
       : this(PersonRepository, mapper, Guid.NewGuid)
    {
    }

    public Task<Model.Person> Execute(RegisterPerson command)
    {
        var Person = mapper.Map<Persistence.Person>(command, o => o.Items["Guid"] = guidFactory());
        PersonRepository.Save(Person);
        return Task.FromResult(mapper.Map<Model.Person>(Person));
    }
}