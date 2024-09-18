using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Tests;
public class EndpointUriBuildingTests
{
    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedNumericIds()
    {
        foreach (var key in Constants.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = Constants.GetEndpointUri(key, Random.Shared.Next(999999).ToString());
            };

            result.Should().NotThrow();
        }
    }

    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedGuidIds()
    {
        foreach (var key in Constants.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = Constants.GetEndpointUri(key, Guid.NewGuid().ToString());
            };

            result.Should().NotThrow();
        }
    }
}