using CO.CDP.EntityVerification.Persistence;
using FluentAssertions;
using static CO.CDP.EntityVerification.Events.Identifier;
using static CO.CDP.EntityVerification.Persistence.Tests.PponFactories;

namespace CO.CDP.EntityVerification.Tests.Events;

public class IdentifierTests
{
    [Fact]
    public void GetPersistenceIdentifiers_ReturnsCorrectCollectionSize()
    {
        // Arrange
        var evIds = new List<CO.CDP.EntityVerification.Events.Identifier>
        {
            new()
            {
                Id = "GB123123123", LegalName = "Acme Ltd", Scheme = "GB-COH", Uri = new Uri("https://www.acme-org.com")
            },
            new()
            {
                Id = "GB123123124", LegalName = "Acme Group Ltd", Scheme = "GB-COH",
                Uri = new Uri("https://www.acme-org.com")
            }
        };
        var newPpon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        // Act
        var result = GetPersistenceIdentifiers(evIds);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetPersistenceIdentifiers_MapsPropertiesCorrectly()
    {
        // Arrange
        var evIds = new List<CO.CDP.EntityVerification.Events.Identifier>
        {
            new()
            {
                Id = "GB123123123", LegalName = "Acme Ltd", Scheme = "GB-COH", Uri = new Uri("https://www.acme-org.com")
            }
        };
        var newPpon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        // Act
        var result = GetPersistenceIdentifiers(evIds);

        // Assert
        result.Should().ContainSingle(i =>
            i.IdentifierId == "GB123123123" &&
            i.LegalName == evIds[0].LegalName &&
            i.Scheme == evIds[0].Scheme &&
            i.Uri == evIds[0].Uri &&
            i.startsOn == DateTimeOffset.MinValue &&
            i.endsOn == null);
    }
}