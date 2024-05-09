using AutoMapper;
using CO.CDP.Common;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Person.WebApi.UseCase;
public class LookupPersonUseCase(IPersonRepository personRepository, IMapper mapper) : IUseCase<string, Model.Person?>
{
    public async Task<Model.Person?> Execute(string urn)
    {
        return await personRepository.FindByUrn(urn)
            .AndThen(mapper.Map<Model.Person>);
    }
}