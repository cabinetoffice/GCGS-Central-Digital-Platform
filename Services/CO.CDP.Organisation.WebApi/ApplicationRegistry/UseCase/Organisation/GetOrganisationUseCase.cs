using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public class GetOrganisationUseCase : IUseCase<Guid, OrganisationDto?>
{
    private readonly IOrganisationRepository _repository;

    public GetOrganisationUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganisationDto?> Execute(Guid id)
    {
        var org = await _repository.GetByIdAsync(id);
        if (org == null) return null;

        return new OrganisationDto(
            org.Id, org.Name, org.Slug, org.Type,
            org.ParentOrganisationId, org.IsActive,
            org.CreatedOn, org.UpdatedOn);
    }
}
