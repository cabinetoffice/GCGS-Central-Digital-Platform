using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Application;

public class UpdateApplicationUseCase : IUseCase<(Guid Id, UpdateApplication Command), bool>
{
    private readonly IApplicationRepository _repository;

    public UpdateApplicationUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Execute((Guid Id, UpdateApplication Command) input)
    {
        var app = await _repository.GetByIdAsync(input.Id)
            ?? throw new KeyNotFoundException($"Application {input.Id} not found.");

        if (input.Command.Name != null)
            app.Name = input.Command.Name;

        if (input.Command.Description != null)
            app.Description = input.Command.Description;

        if (input.Command.Category != null)
            app.Category = input.Command.Category;

        if (input.Command.IsActive.HasValue)
            app.IsActive = input.Command.IsActive.Value;

        await _repository.UpdateAsync(app);
        return true;
    }
}
