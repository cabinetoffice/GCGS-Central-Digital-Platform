using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Tests.Ppon;

namespace CO.CDP.EntityVerification.Tests.Persistence;

public class IdentifierTests
{
    [Fact]
    public void GetPersistenceIdentifiers_ReturnsCorrectCollectionSize()
    {
        // Arrange
        var evIds = new List<Identifier>
        {
            new() { Id = "GB123123123", LegalName = "Acme Ltd", Scheme = "GB-COH", Uri = new Uri("https://www.acme-org.com") },
            new() { Id = "GB123123124", LegalName = "Acme Group Ltd", Scheme = "GB-COH", Uri = new Uri("https://www.acme-org.com") }
        };
        var newPpon = PponTestHelper.GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        // Act
        var result = EntityVerification.Persistence.Identifier.GetPersistenceIdentifiers(evIds, newPpon);

        // Assert
        Assert.Equal(evIds.Count, result.Count);
    }

    [Fact]
    public void GetPersistenceIdentifiers_MapsPropertiesCorrectly()
    {
        // Arrange
        var evIds = new List<Identifier>
        {
            new() { Id = "GB123123123", LegalName = "Acme Ltd", Scheme = "GB-COH", Uri = new Uri("https://www.acme-org.com") }
        };
        var newPpon = PponTestHelper.GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        // Act
        var result = EntityVerification.Persistence.Identifier.GetPersistenceIdentifiers(evIds, newPpon);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var identifier = result.First();
        Assert.Equal(evIds[0].LegalName, identifier.LegalName);
        Assert.Equal(evIds[0].Scheme, identifier.Scheme);
        Assert.Equal(evIds[0].Uri, identifier.Uri);
    }
}