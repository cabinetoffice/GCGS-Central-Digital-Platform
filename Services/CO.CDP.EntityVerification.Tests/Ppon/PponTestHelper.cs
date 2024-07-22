namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponTestHelper
{
    public static EntityVerification.Persistence.Ppon GivenPpon(string? pponId = null)
    {
        return new EntityVerification.Persistence.Ppon
        {
            PponId = pponId ?? Guid.NewGuid().ToString().Replace("-", ""),
            Name = string.Empty,
            OrganisationId = Guid.NewGuid()
        };
    }
}
