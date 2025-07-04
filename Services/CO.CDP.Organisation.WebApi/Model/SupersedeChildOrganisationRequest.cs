namespace CO.CDP.Organisation.WebApi.Model
{
    /// <summary>
    /// Request model for superseding a child organisation relationship
    /// </summary>
    public class SupersedeChildOrganisationRequest
    {
        /// <summary>
        /// The ID of the parent organisation
        /// </summary>
        public Guid ParentOrganisationId { get; set; }

        /// <summary>
        /// The ID of the child organisation to be superseded
        /// </summary>
        public Guid ChildOrganisationId { get; set; }
    }
}
