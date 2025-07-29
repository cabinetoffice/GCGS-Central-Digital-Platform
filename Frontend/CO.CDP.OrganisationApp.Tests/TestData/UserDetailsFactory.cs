using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Tests.TestData;

internal static class UserDetailsFactory
{
    public static UserDetails CreateUserDetails(
        string? userUrn = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phone = null,
        Guid? personId = null)
    {
        return new UserDetails
        {
            UserUrn = userUrn ?? "urn:fdc:gov.uk:2022:68f5c698-b719-4492-9e65-82351325a591",
            FirstName = firstName ?? "Test",
            LastName = lastName ?? "User",
            Email = email ?? "test@example.com",
            Phone = phone ?? "1234567890",
            PersonId = personId
        };
    }
}