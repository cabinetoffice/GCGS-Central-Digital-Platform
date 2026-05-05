using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Application;

public class CreatePermissionUseCase : IUseCase<(Guid AppId, CreatePermission Command), PermissionDto>
{
    private readonly IApplicationRepository _repository;
    private static readonly System.Text.RegularExpressions.Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$");

    public CreatePermissionUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PermissionDto> Execute((Guid AppId, CreatePermission Command) input)
    {
        if (!KebabCaseRegex.IsMatch(input.Command.Name))
            throw new ArgumentException("Permission name must be in kebab-case (e.g., 'can-edit-company').");

        var permission = new CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationPermission
        {
            ApplicationId = input.AppId,
            Name = input.Command.Name,
            Description = input.Command.Description
        };

        var created = await _repository.CreatePermissionAsync(permission);
        return new PermissionDto(created.Id, created.ApplicationId, created.Name, created.Description);
    }
}
