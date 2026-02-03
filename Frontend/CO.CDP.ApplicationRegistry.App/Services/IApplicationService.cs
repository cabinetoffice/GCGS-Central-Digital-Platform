using CO.CDP.ApplicationRegistry.App.Models;

namespace CO.CDP.ApplicationRegistry.App.Services;

public interface IApplicationService
{
    Task<HomeViewModel?> GetHomeViewModelAsync(string organisationSlug, CancellationToken ct = default);

    Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(
        string organisationSlug,
        string? selectedCategory = null,
        string? selectedStatus = null,
        string? searchTerm = null,
        CancellationToken ct = default);

    Task<EnableApplicationViewModel?> GetEnableApplicationViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);

    Task<bool> EnableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);

    Task<EnableApplicationSuccessViewModel?> GetEnableSuccessViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);

    Task<ApplicationDetailsViewModel?> GetApplicationDetailsViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);
}
