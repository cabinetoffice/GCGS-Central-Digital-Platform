using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>>
{
    public async Task<IEnumerable<OrganisationExtended>> Execute(PaginatedOrganisationQuery command)
    {
        return await organisationRepository.GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip)
            .AndThen(mapper.Map<IEnumerable<OrganisationExtended>>);
    }
}