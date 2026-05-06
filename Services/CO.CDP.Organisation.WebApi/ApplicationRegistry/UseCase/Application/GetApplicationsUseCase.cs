using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Application;

public class GetApplicationsUseCase : IUseCase<bool, IEnumerable<ApplicationDto>>
{
    private readonly IApplicationRepository _repository;

    public GetApplicationsUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ApplicationDto>> Execute(bool _)
    {
        var apps = await _repository.GetAllAsync();
        return apps.Select(a => new ApplicationDto(
            a.Id, a.Name, a.ClientId, a.Description,
            a.Category, a.IsActive, a.CreatedOn, a.UpdatedOn));
    }
}
