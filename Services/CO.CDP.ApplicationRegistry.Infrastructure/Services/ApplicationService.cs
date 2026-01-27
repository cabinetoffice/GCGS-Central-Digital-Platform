using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing applications.
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationService(IApplicationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Application?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Application?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByClientIdAsync(clientId, cancellationToken);
    }

    public async Task<IEnumerable<Application>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<Application> CreateAsync(string name, string clientId, string? description, bool isActive, CancellationToken cancellationToken = default)
    {
        // Validate unique client ID
        if (await _repository.ClientIdExistsAsync(clientId, null, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Application), nameof(Application.ClientId), clientId);
        }

        var application = new Application
        {
            Name = name,
            ClientId = clientId,
            Description = description,
            IsActive = isActive
        };

        _repository.Add(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application;
    }

    public async Task<Application> UpdateAsync(int id, string name, string? description, bool isActive, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), id);
        }

        application.Name = name;
        application.Description = description;
        application.IsActive = isActive;

        _repository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), id);
        }

        _repository.Remove(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
