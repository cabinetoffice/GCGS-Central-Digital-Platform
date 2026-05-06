using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;

public record GetOrganisationsQuery(string? Name, string? Type);

public class GetOrganisationsUseCase : IUseCase<GetOrganisationsQuery, IEnumerable<OrganisationDto>>
{
    private readonly IOrganisationRepository _repository;

    public GetOrganisationsUseCase(IOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrganisationDto>> Execute(GetOrganisationsQuery query)
    {
        var orgs = await _repository.GetAllAsync(query.Name, query.Type);

        return orgs.Select(o => new OrganisationDto(
            o.Id, o.Name, o.Slug, o.Type,
            o.ParentOrganisationId, o.IsActive,
            o.CreatedOn, o.UpdatedOn));
    }
}
