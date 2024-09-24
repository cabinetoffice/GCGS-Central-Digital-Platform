using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Person.WebApi.UseCase;

public class GetPersonUseCase(IPersonRepository personRepository, IMapper mapper) : IUseCase<Guid, Model.Person?>
{
    public async Task<Model.Person?> Execute(Guid personId)
    {
        return await personRepository.Find(personId)
            .AndThen(mapper.Map<Model.Person>);
    }
}