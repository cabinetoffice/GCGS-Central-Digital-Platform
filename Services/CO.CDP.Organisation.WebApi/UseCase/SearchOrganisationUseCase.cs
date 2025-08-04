using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class SearchOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationSearchQuery, IEnumerable<Model.OrganisationSearchResult>>
{
    public async Task<IEnumerable<Model.OrganisationSearchResult>> Execute(OrganisationSearchQuery query)
    {
        return await organisationRepository.SearchByName(query.Name, query.Role, query.Limit, query.Threshold, query.IncludePendingRoles)
            .AndThen(organisations => organisations.Select(mapper.Map<Model.OrganisationSearchResult>));        
    }
}