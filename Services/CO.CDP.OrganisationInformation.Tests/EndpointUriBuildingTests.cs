using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Tests;
public class EndpointUriBuildingTests
{
    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedNumericIds()
    {
        foreach (var key in IdentifierSchemes.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = IdentifierSchemes.GetRegistryUri(key, Random.Shared.Next(999999).ToString());
            };

            result.Should().NotThrow();
        }
    }

    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedGuidIds()
    {
        foreach (var key in IdentifierSchemes.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = IdentifierSchemes.GetRegistryUri(key, Guid.NewGuid().ToString());
            };

            result.Should().NotThrow();
        }
    }
}