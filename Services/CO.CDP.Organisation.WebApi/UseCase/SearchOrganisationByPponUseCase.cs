using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class SearchOrganisationByPponUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<OrganisationSearchByPponQuery, (IEnumerable<Model.OrganisationSearchByPponResult> Results, int TotalCount)>
{
    public async Task<(IEnumerable<Model.OrganisationSearchByPponResult> Results, int TotalCount)> Execute(OrganisationSearchByPponQuery query)
    {
        var result = await organisationRepository.SearchByNameOrPpon(query.SearchText, query.Limit, query.Skip, query.OrderBy, query.Threshold);

        var mappedResults = result.Results.Select(mapper.Map<Model.OrganisationSearchByPponResult>);

        return (mappedResults, result.TotalCount);
    }
}