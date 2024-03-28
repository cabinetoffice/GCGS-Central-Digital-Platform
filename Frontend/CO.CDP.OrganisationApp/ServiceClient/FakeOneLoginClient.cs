namespace CO.CDP.OrganisationApp.ServiceClient;

public class FakeOneLoginClient : IOneLoginClient
{
    public async Task<UserProfile?> GetUserInfo()
    {
        var userInfo = new UserProfile
        {
            UserId = $"urn:fdc:gov.uk:2022:{Guid.NewGuid()}",
            Email = "test@example.com",
            EmailVerified = true,
            PhoneNumber = "01406946277",
            PhoneNumberVerified = true
        };

        return await Task.FromResult(userInfo);
    }
}