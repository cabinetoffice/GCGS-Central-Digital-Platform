using CO.CDP.EntityVerification.Events;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponFactories
{
    public static EntityVerification.Persistence.Ppon GivenPpon(string? pponId = null)
    {
        return new EntityVerification.Persistence.Ppon
        {
            IdentifierId = pponId ?? Guid.NewGuid().ToString().Replace("-", ""),
            Name = string.Empty,
            OrganisationId = Guid.NewGuid()
        };
    }
}