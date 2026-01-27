using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing organisations.
/// </summary>
public class OrganisationService : IOrganisationService
{
    private readonly IOrganisationRepository _repository;
    private readonly ISlugGeneratorService _slugGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public OrganisationService(
        IOrganisationRepository repository,
        ISlugGeneratorService slugGenerator,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _slugGenerator = slugGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Organisation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Organisation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _repository.GetBySlugAsync(slug, cancellationToken);
    }

    public async Task<IEnumerable<Organisation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<Organisation> CreateAsync(Guid cdpOrganisationGuid, string name, bool isActive, CancellationToken cancellationToken = default)
    {
        // Check if organisation with this CDP GUID already exists
        var existing = await _repository.GetByCdpGuidAsync(cdpOrganisationGuid, cancellationToken);
        if (existing != null)
        {
            throw new DuplicateEntityException(nameof(Organisation), nameof(Organisation.CdpOrganisationGuid), cdpOrganisationGuid);
        }

        // Generate slug
        var baseSlug = _slugGenerator.GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        // Ensure slug is unique
        while (await _repository.SlugExistsAsync(slug, null, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var organisation = new Organisation
        {
            CdpOrganisationGuid = cdpOrganisationGuid,
            Name = name,
            Slug = slug,
            IsActive = isActive
        };

        _repository.Add(organisation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return organisation;
    }

    public async Task<Organisation> UpdateAsync(int id, string name, bool isActive, CancellationToken cancellationToken = default)
    {
        var organisation = await _repository.GetByIdAsync(id, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), id);
        }

        // If name changed, regenerate slug
        if (organisation.Name != name)
        {
            var baseSlug = _slugGenerator.GenerateSlug(name);
            var slug = baseSlug;
            var counter = 1;

            // Ensure slug is unique (excluding current organisation)
            while (await _repository.SlugExistsAsync(slug, id, cancellationToken))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            organisation.Slug = slug;
        }

        organisation.Name = name;
        organisation.IsActive = isActive;

        _repository.Update(organisation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return organisation;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var organisation = await _repository.GetByIdAsync(id, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), id);
        }

        _repository.Remove(organisation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
