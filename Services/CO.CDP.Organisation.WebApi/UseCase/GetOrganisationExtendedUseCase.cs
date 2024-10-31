using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationExtendedUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<Guid, Model.OrganisationExtended?>
{
    public async Task<Model.OrganisationExtended?> Execute(Guid organisationId)
    {
        return await organisationRepository.FindExtended(organisationId)
            .AndThen(mapper.Map<Model.OrganisationExtended>);
    }
}