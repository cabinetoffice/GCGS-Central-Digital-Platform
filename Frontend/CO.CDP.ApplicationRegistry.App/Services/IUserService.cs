using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Services;

public interface IUserService
{
    Task<UsersViewModel?> GetUsersViewModelAsync(
        string organisationSlug,
        string? selectedRole = null,
        string? selectedApplication = null,
        string? searchTerm = null,
        CancellationToken ct = default);
}
