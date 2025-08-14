using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Use case for retrieving parent organisations of a child organisation
/// </summary>
public interface IGetParentOrganisationsUseCase : IUseCase<Guid, GetParentOrganisationsResponse>
{
}

/// <summary>
/// Implementation of the parent organisations retrieval use case
/// </summary>
public class GetParentOrganisationsUseCase : IGetParentOrganisationsUseCase
{
    private readonly ILogger<GetParentOrganisationsUseCase> _logger;
    private readonly IOrganisationHierarchyRepository _hierarchyRepository;

    /// <summary>
    /// Initializes a new instance of the GetParentOrganisationsUseCase class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hierarchyRepository">Organisation hierarchy repository</param>
    public GetParentOrganisationsUseCase(
        ILogger<GetParentOrganisationsUseCase> logger,
        IOrganisationHierarchyRepository hierarchyRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
    }

    /// <summary>
    /// Retrieves all parent organisations for the specified child organisation
    /// </summary>
    /// <param name="childId">The unique identifier of the child organisation</param>
    /// <returns>Response containing the list of parent organisations</returns>
    public async Task<GetParentOrganisationsResponse> Execute(Guid childId)
    {
        try
        {
            if (childId == Guid.Empty)
            {
                _logger.LogWarning("Invalid child organisation ID provided");
                return CreateFailureResponse();
            }

            var hierarchies = await _hierarchyRepository.GetParentsAsync(childId);
            var parentHierarchies = hierarchies.ToList();

            if (parentHierarchies.Count == 0)
            {
                _logger.LogInformation("No parent hierarchies found for organisation {ChildId}", childId);
                return CreateSuccessResponse([]);
            }

            var parentOrganisations = MapToParentOrganisations(parentHierarchies).ToList();

            _logger.LogInformation(
                "Retrieved {Count} parent organisations for child {ChildId}",
                parentOrganisations.Count,
                childId);

            return CreateSuccessResponse(parentOrganisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent organisations for child {ChildId}", childId);
            return CreateFailureResponse();
        }
    }

    /// <summary>
    /// Maps organisation hierarchies to simplified parent organisation models with only id, name, roles, and identifier
    /// </summary>
    /// <param name="hierarchies">The organisation hierarchies containing parent organisation references</param>
    /// <returns>Collection of simplified parent organisation models</returns>
    private static IEnumerable<OrganisationSummary> MapToParentOrganisations(
        IReadOnlyCollection<OrganisationHierarchy> hierarchies)
    {
        return hierarchies
            .Where(hierarchy => hierarchy.Parent != null)
            .Select(hierarchy => new OrganisationSummary
            {
                Id = hierarchy.Parent!.Guid,
                Name = hierarchy.Parent.Name,
                Roles = hierarchy.Parent.Roles,
                Ppon = GetPponIdentifier(hierarchy.Parent.Identifiers)
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
    /// Creates a success response with the provided parent organisations
    /// </summary>
    /// <param name="parentOrganisations">The parent organisations to include in the response</param>
    /// <returns>A success response containing the parent organisations</returns>
    private static GetParentOrganisationsResponse CreateSuccessResponse(IReadOnlyCollection<OrganisationSummary> parentOrganisations) =>
        new()
        {
            Success = true,
            ParentOrganisations = parentOrganisations
        };

    /// <summary>
    /// Creates a failure response
    /// </summary>
    /// <returns>A failure response</returns>
    private static GetParentOrganisationsResponse CreateFailureResponse() =>
        new()
        {
            Success = false,
            ParentOrganisations = []
        };
}
