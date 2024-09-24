using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetAuthenticationKeyUseCase(IAuthenticationKeyRepository authenticationKeyRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.AuthenticationKey>>
{
    public async Task<IEnumerable<Model.AuthenticationKey>> Execute(Guid organisationId)
    {
        return await authenticationKeyRepository.GetAuthenticationKeys(organisationId)
            .AndThen(mapper.Map<IEnumerable<Model.AuthenticationKey>>);
    }
}