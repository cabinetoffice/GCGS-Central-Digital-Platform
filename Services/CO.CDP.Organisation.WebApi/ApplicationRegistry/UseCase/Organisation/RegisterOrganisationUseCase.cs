using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public class RegisterOrganisationUseCase : IUseCase<CreateOrganisation, OrganisationDto>
{
    private readonly IOrganisationRepository _repository;

    public RegisterOrganisationUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganisationDto> Execute(CreateOrganisation command)
    {
        var slug = SlugGenerator.Generate(command.Name);

        // Ensure slug uniqueness
        var existing = await _repository.GetBySlugAsync(slug);
        if (existing != null)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var organisation = new CO.CDP.ApplicationRegistry.Persistence.Entities.Organisation
        {
            Name = command.Name,
            Slug = slug,
            Type = command.Type,
            ParentOrganisationId = command.ParentOrganisationId
        };

        var created = await _repository.CreateAsync(organisation);

        return new OrganisationDto(
            created.Id, created.Name, created.Slug, created.Type,
            created.ParentOrganisationId, created.IsActive,
            created.CreatedOn, created.UpdatedOn);
    }
}
