using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<string, IEnumerable<Model.Organisation>>
{
    public async Task<IEnumerable<Model.Organisation>> Execute(string userUrn)
    {
        return await organisationRepository.FindByUserUrn(userUrn)
            .AndThen(mapper.Map<IEnumerable<Model.Organisation>>);
    }
}