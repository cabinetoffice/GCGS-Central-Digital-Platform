using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetPersonsUseCase(IPersonRepository personRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.Person>>
{
    public async Task<IEnumerable<Model.Person>> Execute(Guid organisationId)
    {
        return await personRepository.FindByOrganisation(organisationId)
            .AndThen(mapper.Map<IEnumerable<Model.Person>>);
    }
}