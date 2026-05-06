using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Application;

public class GetApplicationUseCase : IUseCase<Guid, ApplicationDto?>
{
    private readonly IApplicationRepository _repository;

    public GetApplicationUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto?> Execute(Guid id)
    {
        var app = await _repository.GetByIdAsync(id);
        if (app == null) return null;

        return new ApplicationDto(
            app.Id, app.Name, app.ClientId, app.Description,
            app.Category, app.IsActive, app.CreatedOn, app.UpdatedOn);
    }
}
