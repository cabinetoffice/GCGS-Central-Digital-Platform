using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Services;

public interface IApplicationService
{
    Task<HomeViewModel?> GetHomeViewModelAsync(int orgId, CancellationToken ct = default);

    Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(int orgId, CancellationToken ct = default);
}
