using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;

using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class LookupOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationQuery, Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute(OrganisationQuery query)
    {
        if (query.TryGetIdentifier(out var scheme, out var id))
        {
            return await organisationRepository.FindByIdentifier(scheme, id)
                                   .AndThen(mapper.Map<Model.Organisation>);
        }

        if (query.TryGetName(out var name))
        {
            return await organisationRepository.FindByName(name)
                                   .AndThen(mapper.Map<Model.Organisation>);
        }

        throw new InvalidQueryException("Both name and identifier are missing from the request.");
    }
}