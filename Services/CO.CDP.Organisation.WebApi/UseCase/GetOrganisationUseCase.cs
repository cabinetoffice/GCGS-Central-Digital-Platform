using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<Guid, Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute(Guid organisationId)
    {
        return await organisationRepository.Find(organisationId)
            .AndThen(mapper.Map<Model.Organisation>);
    }
}