using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Use case for creating parent-child relationships between organisations
/// </summary>
public interface
    ICreateParentChildRelationshipUseCase : IUseCase<CreateParentChildRelationshipRequest,
    ParentChildRelationshipResult>
{
}

/// <summary>
/// Implementation of the parent-child relationship creation use case
/// </summary>
public class CreateParentChildRelationshipUseCase : ICreateParentChildRelationshipUseCase
{
    private readonly ILogger<CreateParentChildRelationshipUseCase> _logger;

    /// <summary>
    /// Initialises a new instance of the CreateParentChildRelationshipUseCase class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public CreateParentChildRelationshipUseCase(ILogger<CreateParentChildRelationshipUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a parent-child relationship between two organisations
    /// </summary>
    /// <param name="request">Request containing parent and child organisation IDs and role</param>
    /// <returns>Result of the relationship creation operation</returns>
    public Task<ParentChildRelationshipResult> Execute(CreateParentChildRelationshipRequest request)
    {
        try
        {
            if (request.ParentId == Guid.Empty || request.ChildId == Guid.Empty)
            {
                _logger.LogWarning("Invalid organisation IDs provided for parent-child relationship");
                return Task.FromResult(new ParentChildRelationshipResult { Success = false });
            }

            if (request.ParentId == request.ChildId)
            {
                _logger.LogWarning("Parent and child organisation IDs cannot be the same");
                return Task.FromResult(new ParentChildRelationshipResult { Success = false });
            }

            var relationshipId = Guid.NewGuid();
            _logger.LogInformation(
                "Created parent-child relationship {RelationshipId} between {ParentId} and {ChildId} with role {Role}",
                relationshipId, request.ParentId, request.ChildId, request.Role);

            return Task.FromResult(new ParentChildRelationshipResult
            {
                Success = true,
                RelationshipId = relationshipId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent-child relationship between {ParentId} and {ChildId}",
                request.ParentId, request.ChildId);
            return Task.FromResult(new ParentChildRelationshipResult { Success = false });
        }
    }
}