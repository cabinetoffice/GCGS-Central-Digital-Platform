using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using System.Text.RegularExpressions;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Application;

public class RegisterApplicationUseCase : IUseCase<CreateApplication, ApplicationDto>
{
    private readonly IApplicationRepository _repository;

    public RegisterApplicationUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto> Execute(CreateApplication command)
    {
        var application = new CO.CDP.ApplicationRegistry.Persistence.Entities.Application
        {
            Name = command.Name,
            ClientId = command.ClientId,
            Description = command.Description,
            Category = command.Category
        };

        var created = await _repository.CreateAsync(application);

        return new ApplicationDto(
            created.Id, created.Name, created.ClientId, created.Description,
            created.Category, created.IsActive, created.CreatedOn, created.UpdatedOn);
    }
}
