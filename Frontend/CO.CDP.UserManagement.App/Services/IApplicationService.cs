using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Services;

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

    Task<Result<ServiceFailure, ServiceOutcome>> EnableApplicationAsync(
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

    Task<DisableApplicationViewModel?> GetDisableApplicationViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> DisableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default);
}
