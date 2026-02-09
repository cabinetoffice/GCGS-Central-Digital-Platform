using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing organisation-application relationships.
/// </summary>
public class OrganisationApplicationService : IOrganisationApplicationService
{
    private readonly IOrganisationApplicationRepository _organisationApplicationRepository;
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrganisationApplicationService> _logger;

    public OrganisationApplicationService(
        IOrganisationApplicationRepository organisationApplicationRepository,
        IOrganisationRepository organisationRepository,
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrganisationApplicationService> logger)
    {
        _organisationApplicationRepository = organisationApplicationRepository;
        _organisationRepository = organisationRepository;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<OrganisationApplication>> GetByOrganisationIdAsync(
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting applications for organisation ID: {OrganisationId}", organisationId);

        // Verify organisation exists
        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), organisationId);
        }

        return await _organisationApplicationRepository.GetByOrganisationIdAsync(organisationId, cancellationToken);
    }

    public async Task<IEnumerable<OrganisationApplication>> GetApplicationsByUserAsync(
        string userPrincipalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting applications for user: {UserPrincipalId}", userPrincipalId);

        return await _organisationApplicationRepository.GetApplicationsByUserAsync(userPrincipalId, cancellationToken);
    }

    public async Task<OrganisationApplication> EnableApplicationAsync(
        int organisationId,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enabling application ID: {ApplicationId} for organisation ID: {OrganisationId}",
            applicationId, organisationId);

        // Verify organisation exists
        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), organisationId);
        }

        // Verify application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), applicationId);
        }

        // Check if relationship already exists
        var existingRelationship = await _organisationApplicationRepository
            .GetByOrganisationAndApplicationAsync(organisationId, applicationId, cancellationToken);

        if (existingRelationship != null)
        {
            // If exists but inactive, reactivate it
            if (!existingRelationship.IsActive)
            {
                existingRelationship.IsActive = true;
                existingRelationship.EnabledAt = DateTimeOffset.UtcNow;
                existingRelationship.DisabledAt = null;
                existingRelationship.DisabledBy = null;

                _organisationApplicationRepository.Update(existingRelationship);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Application ID: {ApplicationId} reactivated for organisation ID: {OrganisationId}",
                    applicationId, organisationId);
                return existingRelationship;
            }

            throw new DuplicateEntityException(
                nameof(OrganisationApplication),
                $"Organisation {organisationId} and Application {applicationId}",
                $"{organisationId}-{applicationId}");
        }

        var organisationApplication = new OrganisationApplication
        {
            OrganisationId = organisationId,
            ApplicationId = applicationId,
            IsActive = true,
            EnabledAt = DateTimeOffset.UtcNow
        };

        _organisationApplicationRepository.Add(organisationApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application ID: {ApplicationId} enabled for organisation ID: {OrganisationId} with ID: {OrganisationApplicationId}",
            applicationId, organisationId, organisationApplication.Id);
        return organisationApplication;
    }

    public async Task DisableApplicationAsync(
        int organisationId,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disabling application ID: {ApplicationId} for organisation ID: {OrganisationId}",
            applicationId, organisationId);

        var organisationApplication = await _organisationApplicationRepository
            .GetByOrganisationAndApplicationAsync(organisationId, applicationId, cancellationToken);

        if (organisationApplication == null)
        {
            throw new EntityNotFoundException(
                nameof(OrganisationApplication),
                $"Organisation {organisationId} and Application {applicationId}");
        }

        organisationApplication.IsActive = false;
        organisationApplication.DisabledAt = DateTimeOffset.UtcNow;

        _organisationApplicationRepository.Update(organisationApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application ID: {ApplicationId} disabled for organisation ID: {OrganisationId}",
            applicationId, organisationId);
    }
}
