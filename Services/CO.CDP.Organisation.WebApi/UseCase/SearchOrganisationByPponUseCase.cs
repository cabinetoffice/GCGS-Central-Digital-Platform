using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class SearchOrganisationByPponUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationSearchByPponQuery, IEnumerable<Model.OrganisationSearchByPponResult>>
{
    public async Task<IEnumerable<Model.OrganisationSearchByPponResult>> Execute(OrganisationSearchByPponQuery query)
    {
        return await organisationRepository.SearchByNameOrPpon(query.Name, query.Limit, query.Skip)
            .AndThen(organisations => organisations.Select(mapper.Map<Model.OrganisationSearchByPponResult>));
    }
}