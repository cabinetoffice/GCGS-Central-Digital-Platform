using AutoMapper;
using CO.CDP.Common;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Person.WebApi.UseCase;

public class GetPersonUseCase(IPersonRepository personRepository, IMapper mapper) : IUseCase<Guid, Model.Person?>
{
    public async Task<Model.Person?> Execute(Guid tenantId)
    {
        return await personRepository.Find(tenantId)
            .AndThen(mapper.Map<Model.Person>);
    }
}