using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Person.WebApi.UseCase;
public class LookupPersonUseCase(IPersonRepository personRepository, IMapper mapper) : IUseCase<string, Model.Person?>
{
    public async Task<Model.Person?> Execute(string name)
    {
        return await personRepository.FindByEmail(name)
            .AndThen(mapper.Map<Model.Person>);
    }
}