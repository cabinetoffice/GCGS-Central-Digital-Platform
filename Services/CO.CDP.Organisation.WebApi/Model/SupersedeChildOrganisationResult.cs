using System;

namespace CO.CDP.Organisation.WebApi.Model
{
    /// <summary>
    /// Result model for superseding a child organisation relationship
    /// </summary>
    public class SupersedeChildOrganisationResult
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Indicates whether the relationship was not found
        /// </summary>
        public bool NotFound { get; set; }

        /// <summary>
        /// The ID of the parent organisation
        /// </summary>
        public Guid ParentOrganisationId { get; set; }

        /// <summary>
        /// The ID of the child organisation that was superseded
        /// </summary>
        public Guid ChildOrganisationId { get; set; }

        /// <summary>
        /// The date the relationship was superseded
        /// </summary>
        public DateTimeOffset? SupersededDate { get; set; }
    }
}
