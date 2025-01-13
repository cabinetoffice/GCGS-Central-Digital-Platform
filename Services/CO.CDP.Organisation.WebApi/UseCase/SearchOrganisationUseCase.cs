using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class SearchOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationSearchQuery, IEnumerable<Model.Organisation>>
{
    public async Task<IEnumerable<Model.Organisation>> Execute(OrganisationSearchQuery query)
    {
        return await organisationRepository.SearchByName(query.Name, query.Role, query.Limit)
            .AndThen(organisations => organisations.Select(mapper.Map<Model.Organisation>));        
    }
}