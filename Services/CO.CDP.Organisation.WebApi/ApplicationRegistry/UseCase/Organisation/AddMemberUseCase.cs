using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public class AddMemberUseCase : IUseCase<(Guid OrgId, AddMember Command), bool>
{
    private readonly IOrganisationRepository _repository;

    public AddMemberUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Execute((Guid OrgId, AddMember Command) input)
    {
        var org = await _repository.GetByIdAsync(input.OrgId)
            ?? throw new KeyNotFoundException($"Organisation {input.OrgId} not found.");

        var existing = await _repository.GetMemberAsync(input.OrgId, input.Command.UserPrincipalId);
        if (existing != null)
            throw new InvalidOperationException("User is already a member of this organisation.");

        var membership = new UserOrganisationMembership
        {
            UserPrincipalId = input.Command.UserPrincipalId,
            OrganisationId = input.OrgId,
            OrganisationRole = input.Command.OrganisationRole
        };

        await _repository.AddMemberAsync(membership);
        return true;
    }
}
