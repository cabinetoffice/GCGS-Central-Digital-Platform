namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponFactories
{
    public static EntityVerification.Persistence.Ppon GivenPpon(
        string? pponId = null,
        string? name = null
    )
    {
        var identifierId = pponId ?? Guid.NewGuid().ToString().Replace("-", "");
        return new EntityVerification.Persistence.Ppon
        {
            IdentifierId = identifierId,
            Name = name ?? $"PPON {identifierId}",
            OrganisationId = Guid.NewGuid()
        };
    }

    public static EntityVerification.Persistence.Ppon GivenPponWithIdentifier(string? pponId = null)
    {
        var ppon = GivenPpon(pponId);

        ppon.Identifiers =
        [
            new EntityVerification.Persistence.Identifier() {
                LegalName  = "Acme Ltd",
                Scheme = "GB-COH",
                IdentifierId = "CO-1234567" },
        ];

        return ppon;
    }
}