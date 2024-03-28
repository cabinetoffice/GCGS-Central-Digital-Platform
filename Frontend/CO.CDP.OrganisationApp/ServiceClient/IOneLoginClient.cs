using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.ServiceClient;

public interface IOneLoginClient
{
    Task<UserProfile?> GetUserInfo();
}

public class UserProfile
{
    [JsonPropertyName("sub")]
    public required string UserId { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("email_verified")]
    public required bool EmailVerified { get; set; }

    [JsonPropertyName("phone_number")]
    public required string PhoneNumber { get; set; }

    [JsonPropertyName("phone_number_verified")]
    public required bool PhoneNumberVerified { get; set; }
}