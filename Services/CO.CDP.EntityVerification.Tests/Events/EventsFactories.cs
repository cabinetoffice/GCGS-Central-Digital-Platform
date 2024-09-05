using CO.CDP.EntityVerification.Events;

namespace CO.CDP.EntityVerification.Tests.Events;

public static class EventsFactories
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
            Scheme = IdentifierSchemes.Ppon,
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
                Id = "92be415e5985421087bc8fee8c97d338",
                LegalName = "Acme Ltd",
                Scheme = IdentifierSchemes.Ppon,
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