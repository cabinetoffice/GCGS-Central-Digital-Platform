using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;

namespace CO.CDP.Organisation.WebApi.UseCase
{
    /// <summary>
    /// Use case for superseding a child organisation relationship
    /// </summary>
    public interface
        ISupersedeChildOrganisationUseCase : IUseCase<SupersedeChildOrganisationRequest,
        SupersedeChildOrganisationResult>
    {
    }

    /// <summary>
    /// Implementation of the child organisation superseding use case
    /// </summary>
    public class SupersedeChildOrganisationUseCase : ISupersedeChildOrganisationUseCase
    {
        private readonly ILogger<SupersedeChildOrganisationUseCase> _logger;
        private readonly IOrganisationHierarchyRepository _hierarchyRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupersedeChildOrganisationUseCase"/> class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="hierarchyRepository">The organisation hierarchy repository.</param>
        public SupersedeChildOrganisationUseCase(
            ILogger<SupersedeChildOrganisationUseCase> logger,
            IOrganisationHierarchyRepository hierarchyRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
        }

        /// <summary>
        /// Executes the use case to supersede a child organisation relationship.
        /// </summary>
        /// <param name="request">The request containing the parent and child organisation IDs.</param>
        /// <returns>The result of the supersede operation.</returns>
        public async Task<SupersedeChildOrganisationResult> Execute(SupersedeChildOrganisationRequest request)
        {
            try
            {
                if (request is null)
                {
                    _logger.LogWarning("Supersede child organisation request is null");
                    return new SupersedeChildOrganisationResult
                    {
                        Success = false,
                        NotFound = false
                    };
                }

                var children = await _hierarchyRepository.GetChildrenAsync(request.ParentOrganisationId);
                var relationship = children.FirstOrDefault(r =>
                    r.Child?.Guid == request.ChildOrganisationId &&
                    r.SupersededOn == null);

                if (relationship == null)
                {
                    _logger.LogWarning(
                        "No active relationship found between parent {ParentId} and child {ChildId}",
                        request.ParentOrganisationId, request.ChildOrganisationId);

                    return new SupersedeChildOrganisationResult
                    {
                        Success = false,
                        NotFound = true,
                        ParentOrganisationId = request.ParentOrganisationId,
                        ChildOrganisationId = request.ChildOrganisationId
                    };
                }

                var result = await _hierarchyRepository.SupersedeRelationshipAsync(relationship.RelationshipId);

                if (result)
                {
                    _logger.LogInformation(
                        "Successfully superseded relationship {RelationshipId} between parent {ParentId} and child {ChildId}",
                        relationship.RelationshipId, request.ParentOrganisationId, request.ChildOrganisationId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to supersede relationship {RelationshipId} between parent {ParentId} and child {ChildId}",
                        relationship.RelationshipId, request.ParentOrganisationId, request.ChildOrganisationId);
                }

                return new SupersedeChildOrganisationResult
                {
                    Success = result,
                    NotFound = false,
                    ParentOrganisationId = request.ParentOrganisationId,
                    ChildOrganisationId = request.ChildOrganisationId,
                    SupersededDate = result ? DateTimeOffset.UtcNow : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error superseding relationship between parent {ParentId} and child {ChildId}",
                    request.ParentOrganisationId, request.ChildOrganisationId);

                return new SupersedeChildOrganisationResult
                {
                    Success = false,
                    NotFound = false,
                    ParentOrganisationId = request.ParentOrganisationId,
                    ChildOrganisationId = request.ChildOrganisationId
                };
            }
        }
    }
}