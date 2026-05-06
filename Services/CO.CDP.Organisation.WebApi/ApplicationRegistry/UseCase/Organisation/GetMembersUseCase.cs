using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public class GetMembersUseCase : IUseCase<Guid, IEnumerable<MemberDto>>
{
    private readonly IOrganisationRepository _repository;

    public GetMembersUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MemberDto>> Execute(Guid organisationId)
    {
        var members = await _repository.GetMembersAsync(organisationId);
        return members.Select(m => new MemberDto(
            m.Id, m.UserPrincipalId, m.OrganisationRole,
            m.JoinedAt, m.IsActive));
    }
}
