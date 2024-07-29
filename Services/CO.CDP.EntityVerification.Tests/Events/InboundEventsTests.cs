using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Tests.Ppon;
using FluentAssertions;

namespace CO.CDP.EntityVerification.Tests.Events;

public class InboundEventsTests
{
    [Fact]
    public void FindIdentifierByScheme_ShouldReturnCorrectIdentifier()
    {
        var sicIdentifier = new Identifier { Scheme = "SIC", Id = "01230", LegalName = "Acme Ltd" };
        var identifiers = new List<Identifier>() { sicIdentifier };
        var inboundEvents = EventsFactories.GivenOrganisationUpdatedEvent();

        inboundEvents.AdditionalIdentifiers.AddRange(identifiers);

        var result = inboundEvents.FindIdentifierByScheme("SIC");

        result.Should().NotBeNull();
        result!.Scheme.Should().Be(sicIdentifier.Scheme);
        result.Id.Should().Be(sicIdentifier.Id);
        result.LegalName.Should().Be(sicIdentifier.LegalName);
    }

    [Fact]
    public void FindIdentifierByScheme_ShouldReturnNull_WhenSchemeNotFound()
    {
        var identifiers = new List<Identifier>
        {
            new Identifier { Scheme = "SIC", Id = "01230", LegalName = "Acme Ltd" }
        };

        var inboundEvents = EventsFactories.GivenOrganisationUpdatedEvent();
        var result = inboundEvents.FindIdentifierByScheme("NonExistentScheme");

        result.Should().BeNull();
    }
}