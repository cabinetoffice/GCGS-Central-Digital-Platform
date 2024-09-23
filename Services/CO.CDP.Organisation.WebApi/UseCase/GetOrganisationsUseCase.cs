using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>>
{
    // public async Task<IEnumerable<Model.Organisation>> Execute(string userUrn)
    // {
    //     return await organisationRepository.FindByUserUrn(userUrn)
    //         .AndThen(mapper.Map<IEnumerable<Model.Organisation>>);
    // }

    public async Task<IEnumerable<OrganisationExtended>> Execute(PaginatedOrganisationQuery command)
    {
        return await organisationRepository.Get(command.Type)
            .AndThen(mapper.Map<IEnumerable<OrganisationExtended>>);
    }
}