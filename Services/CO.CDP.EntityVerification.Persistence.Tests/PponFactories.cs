namespace CO.CDP.EntityVerification.Persistence.Tests;

public class PponFactories
{
    public static Ppon GivenPpon(
        string? pponId = null,
        string? name = null
    )
    {
        var identifierId = pponId ?? Guid.NewGuid().ToString().Replace("-", "");
        return new Ppon
        {
            IdentifierId = identifierId,
            Name = name ?? $"PPON {identifierId}",
            OrganisationId = Guid.NewGuid()
        };
    }

    public static Ppon GivenPponWithIdentifier(string? pponId = null)
    {
        var ppon = GivenPpon(pponId);

        ppon.Identifiers =
        [
            new Identifier() {
                LegalName  = "Acme Ltd",
                Scheme = "GB-COH",
                IdentifierId = "CO-1234567" },
        ];

        return ppon;
    }
}