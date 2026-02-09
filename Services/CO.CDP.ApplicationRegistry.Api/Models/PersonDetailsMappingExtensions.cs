using CO.CDP.ApplicationRegistry.Core.Models;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Extension methods for mapping PersonDetails to response models.
/// </summary>
public static class PersonDetailsMappingExtensions
{
    /// <summary>
    /// Converts PersonDetails to PersonDetailsResponse.
    /// </summary>
    /// <param name="personDetails">The person details.</param>
    /// <returns>The person details response.</returns>
    public static PersonDetailsResponse ToResponse(this PersonDetails personDetails)
    {
        return new PersonDetailsResponse
        {
            FirstName = personDetails.FirstName,
            LastName = personDetails.LastName,
            Email = personDetails.Email,
            CdpPersonId = personDetails.CdpPersonId
        };
    }
}
