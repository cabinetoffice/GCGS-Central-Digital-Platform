using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public class UpdateOrganisationUseCase : IUseCase<(Guid Id, UpdateOrganisation Command), bool>
{
    private readonly IOrganisationRepository _repository;

    public UpdateOrganisationUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Execute((Guid Id, UpdateOrganisation Command) input)
    {
        var org = await _repository.GetByIdAsync(input.Id)
            ?? throw new KeyNotFoundException($"Organisation {input.Id} not found.");

        if (input.Command.Name != null)
        {
            org.Name = input.Command.Name;
            org.Slug = SlugGenerator.Generate(input.Command.Name);
        }

        if (input.Command.Type != null)
            org.Type = input.Command.Type;

        if (input.Command.ParentOrganisationId.HasValue)
            org.ParentOrganisationId = input.Command.ParentOrganisationId;

        if (input.Command.IsActive.HasValue)
            org.IsActive = input.Command.IsActive.Value;

        await _repository.UpdateAsync(org);
        return true;
    }
}
