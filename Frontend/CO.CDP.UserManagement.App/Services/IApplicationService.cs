using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Services;

public interface IApplicationService
{
    Task<HomeViewModel?> GetHomeViewModelAsync(Guid id, CancellationToken ct = default);

    Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(
        Guid id,
        string? selectedCategory = null,
        string? selectedStatus = null,
        string? searchTerm = null,
        CancellationToken ct = default);

    Task<EnableApplicationViewModel?> GetEnableApplicationViewModelAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> EnableApplicationAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);

    Task<EnableApplicationSuccessViewModel?> GetEnableSuccessViewModelAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);

    Task<ApplicationDetailsViewModel?> GetApplicationDetailsViewModelAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);

    Task<DisableApplicationViewModel?> GetDisableApplicationViewModelAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> DisableApplicationAsync(
        Guid id,
        string applicationSlug,
        CancellationToken ct = default);
}
