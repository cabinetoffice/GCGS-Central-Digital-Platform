using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Services;

public interface IApplicationService
{
    Task<HomeViewModel?> GetHomeViewModelAsync(string organisationSlug, CancellationToken ct = default);

    Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(
        string organisationSlug,
        string? selectedCategory = null,
        string? selectedStatus = null,
        CancellationToken ct = default);
}
