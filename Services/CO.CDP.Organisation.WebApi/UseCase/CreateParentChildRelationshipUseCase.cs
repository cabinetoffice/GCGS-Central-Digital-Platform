using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Use case for creating parent-child relationships between organisations
/// </summary>
public interface
    ICreateParentChildRelationshipUseCase : IUseCase<CreateParentChildRelationshipRequest,
    CreateParentChildRelationshipResult>
{
}

/// <summary>
/// Implementation of the parent-child relationship creation use case
/// </summary>
public class CreateParentChildRelationshipUseCase : ICreateParentChildRelationshipUseCase
{
    private readonly ILogger<CreateParentChildRelationshipUseCase> _logger;
    private readonly IOrganisationHierarchyRepository _repository;

    /// <summary>
    /// Initialises a new instance of the CreateParentChildRelationshipUseCase class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="repository">Organisation hierarchy repository</param>
    public CreateParentChildRelationshipUseCase(
        ILogger<CreateParentChildRelationshipUseCase> logger,
        IOrganisationHierarchyRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Creates a parent-child relationship between two organisations
    /// </summary>
    /// <param name="request">Request containing parent and child organisation IDs</param>
    /// <returns>Result of the relationship creation operation</returns>
    public async Task<CreateParentChildRelationshipResult> Execute(CreateParentChildRelationshipRequest request)
    {
        try
        {
            if (request.ParentId == Guid.Empty || request.ChildId == Guid.Empty)
            {
                _logger.LogWarning("Invalid organisation IDs provided for parent-child relationship");
                return new CreateParentChildRelationshipResult { Success = false };
            }

            if (request.ParentId == request.ChildId)
            {
                _logger.LogWarning("Parent and child organisation IDs cannot be the same");
                return new CreateParentChildRelationshipResult { Success = false };
            }

            var relationshipId = await _repository.CreateRelationshipAsync(
                request.ParentId,
                request.ChildId);

            _logger.LogInformation(
                "Created parent-child relationship {RelationshipId} between {ParentId} and {ChildId}",
                relationshipId, request.ParentId, request.ChildId);

            return new CreateParentChildRelationshipResult
            {
                Success = true,
                RelationshipId = relationshipId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent-child relationship between {ParentId} and {ChildId}",
                request.ParentId, request.ChildId);
            return new CreateParentChildRelationshipResult { Success = false };
        }
    }
}