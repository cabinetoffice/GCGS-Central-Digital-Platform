using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class FindOrganisationByAdminEmailUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationsByAdminEmailQuery, IEnumerable<Model.OrganisationSearchResult>>
{
    public async Task<IEnumerable<Model.OrganisationSearchResult>> Execute(OrganisationsByAdminEmailQuery query)
    {
        return await organisationRepository.FindByAdminEmail(query.Email, query.Role, query.Limit)
            .AndThen(organisations => organisations.Select(mapper.Map<Model.OrganisationSearchResult>));
    }
}