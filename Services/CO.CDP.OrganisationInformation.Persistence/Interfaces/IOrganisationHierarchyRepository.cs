namespace CO.CDP.OrganisationInformation.Persistence.Interfaces
{
    /// <summary>
    /// Repository interface for operations on organisation hierarchies
    /// </summary>
    public interface IOrganisationHierarchyRepository
    {
        /// <summary>
        /// Creates a new parent-child relationship between organisations
        /// </summary>
        /// <param name="parentId">The ID of the parent organisation</param>
        /// <param name="childId">The ID of the child organisation</param>
        /// <param name="createdBy">The user URN of the person creating the relationship</param>
        /// <returns>The ID of the newly created relationship</returns>
        Task<Guid> CreateRelationshipAsync(Guid parentId, Guid childId, string? createdBy = null);

        /// <summary>
        /// Gets all active relationships where the specified organisation is a parent
        /// </summary>
        /// <param name="parentId">The parent organisation ID</param>
        /// <returns>A collection of active organisation hierarchies</returns>
        Task<IEnumerable<OrganisationHierarchy>> GetChildrenAsync(Guid parentId);

        /// <summary>
        /// Marks a relationship as superseded (logical delete)
        /// </summary>
        /// <param name="id">The relationship ID</param>
        /// <param name="supersededBy">The user URN of the person superseding the relationship</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SupersedeRelationshipAsync(Guid id, string? supersededBy = null);

        /// <summary>
        /// Gets all active relationships where the specified organisation is a child
        /// </summary>
        /// <param name="childId">The child organisation ID</param>
        /// <returns>A collection of active organisation hierarchies</returns>
        Task<IEnumerable<OrganisationHierarchy>> GetParentsAsync(Guid childId);
    }
}