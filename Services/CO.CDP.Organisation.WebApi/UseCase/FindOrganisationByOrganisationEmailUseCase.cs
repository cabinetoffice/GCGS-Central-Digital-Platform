using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class FindOrganisationByOrganisationEmailUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationsByOrganisationEmailQuery, IEnumerable<Model.OrganisationSearchResult>>
{
    public async Task<IEnumerable<Model.OrganisationSearchResult>> Execute(OrganisationsByOrganisationEmailQuery query)
    {
        return await organisationRepository.FindByOrganisationEmail(query.Email, query.Role, query.Limit)
            .AndThen(organisations => organisations.Select(mapper.Map<Model.OrganisationSearchResult>));
    }
}