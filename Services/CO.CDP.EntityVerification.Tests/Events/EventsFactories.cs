using CO.CDP.EntityVerification.Events;

namespace CO.CDP.EntityVerification.Tests.Events;

public class EventsFactories
{
    public static OrganisationRegistered GivenOrganisationRegisteredEvent(
        Guid? id = null,
        string name = "Acme Ltd"
    )
    {
        return new OrganisationRegistered
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Identifier = new Identifier
            {
                Id = "93433423432",
                LegalName = name,
                Scheme = "GB-COH",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new Identifier
                {
                    Id = "GB123123123",
                    LegalName = name,
                    Scheme = "VAT",
                    Uri = null
                }
            ],
            Roles = ["supplier"]
        };
    }

    public static Identifier CreatePponIdentifier()
    {
        return new Identifier
        {
            Id = "GB123123123",
            LegalName = "Acme Ltd",
            Scheme = Identifier.PponSchemeName,
            Uri = null
        };
    }

    public static OrganisationUpdated GivenOrganisationUpdatedEvent()
    {
        return new OrganisationUpdated
        {
            Id = Guid.NewGuid(),
            Name = "Acme",
            Identifier = new Identifier
            {
                Id = "93433423432",
                LegalName = "Acme Ltd",
                Scheme = "GB-COH",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new Identifier
                {
                    Id = "GB123123123",
                    LegalName = "Acme Ltd",
                    Scheme = "VAT",
                    Uri = null
                }
            ],
            Roles = ["supplier"]
        };
    }
}
