using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Use case for retrieving child organisations of a parent organisation
/// </summary>
public interface IGetChildOrganisationsUseCase : IUseCase<Guid, GetChildOrganisationsResponse>
{
}

/// <summary>
/// Implementation of the child organisations retrieval use case
/// </summary>
public class GetChildOrganisationsUseCase : IGetChildOrganisationsUseCase
{
    private readonly ILogger<GetChildOrganisationsUseCase> _logger;
    private readonly IOrganisationHierarchyRepository _hierarchyRepository;

    /// <summary>
    /// Initializes a new instance of the GetChildOrganisationsUseCase class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hierarchyRepository">Organisation hierarchy repository</param>
    public GetChildOrganisationsUseCase(
        ILogger<GetChildOrganisationsUseCase> logger,
        IOrganisationHierarchyRepository hierarchyRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
    }

    /// <summary>
    /// Retrieves all child organisations for the specified parent organisation
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent organisation</param>
    /// <returns>Response containing the list of child organisations</returns>
    public async Task<GetChildOrganisationsResponse> Execute(Guid parentId)
    {
        try
        {
            if (parentId == Guid.Empty)
            {
                _logger.LogWarning("Invalid parent organisation ID provided");
                return CreateFailureResponse();
            }

            var hierarchies = await _hierarchyRepository.GetChildrenAsync(parentId);

            var childHierarchies = hierarchies.ToList();

            if (childHierarchies.Count == 0)
            {
                _logger.LogInformation("No child hierarchies found for parent {ParentId}", parentId);
                return CreateSuccessResponse([]);
            }

            var childOrganisations = MapToChildOrganisations(childHierarchies).ToList();

            _logger.LogInformation(
                "Retrieved {Count} child organisations for parent {ParentId}",
                childOrganisations.Count,
                parentId);

            return CreateSuccessResponse(childOrganisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child organisations for parent {ParentId}", parentId);
            return CreateFailureResponse();
        }
    }

    /// <summary>
    /// Maps organisation hierarchies to simplified child organisation models with only id, name, roles, and identifier
    /// </summary>
    /// <param name="hierarchies">The organisation hierarchies containing child organisation references</param>
    /// <returns>Collection of simplified child organisation models</returns>
    private static IEnumerable<OrganisationSummary> MapToChildOrganisations(
        IReadOnlyCollection<OrganisationHierarchy> hierarchies)
    {
        return hierarchies
            .Where(hierarchy => hierarchy.Child != null)
            .Select(hierarchy => new OrganisationSummary
            {
                Id = hierarchy.Child!.Guid,
                Name = hierarchy.Child.Name,
                Roles = hierarchy.Child.Roles,
                Ppon = GetPponIdentifier(hierarchy.Child.Identifiers)
            })
            .ToList();
    }

    /// <summary>
    /// Gets the PPON identifier from a collection of identifiers by filtering for scheme "GB-PPON"
    /// </summary>
    /// <param name="identifiers">Collection of identifiers</param>
    /// <returns>The PPON identifier or empty if none exists</returns>
    private static string GetPponIdentifier(IEnumerable<Identifier> identifiers)
    {
        const string pponScheme = "GB-PPON";

        var firstMatch = identifiers.FirstOrDefault(i =>
            string.Equals(i.Scheme, pponScheme, StringComparison.OrdinalIgnoreCase));

        return firstMatch?.IdentifierId ?? string.Empty;
    }

    /// <summary>
    /// Creates a success response with the provided child organisations
    /// </summary>
    /// <param name="childOrganisations">The child organisations to include in the response</param>
    /// <returns>A success response containing the child organisations</returns>
    private static GetChildOrganisationsResponse CreateSuccessResponse(IReadOnlyCollection<OrganisationSummary> childOrganisations) =>
        new()
        {
            Success = true,
            ChildOrganisations = childOrganisations
        };

    /// <summary>
    /// Creates a failure response
    /// </summary>
    /// <returns>A failure response</returns>
    private static GetChildOrganisationsResponse CreateFailureResponse() =>
        new()
        {
            Success = false,
            ChildOrganisations = []
        };
}
