using CO.CDP.DataSharing.WebApi;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationInformation.Tests;
public class EndpointUriBuildingTests
{
    private readonly Mock<IConfigurationService> _configurationService = new();

    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedNumericIds()
    {
        foreach (var key in IdentifierSchemes.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = IdentifierSchemes.GetRegistryUri(_configurationService.Object.GetOrganisationApiHostUrl(), key, Random.Shared.Next(999999).ToString());
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
                var uri = IdentifierSchemes.GetRegistryUri(_configurationService.Object.GetOrganisationApiHostUrl(), key, Guid.NewGuid().ToString());
            };

            result.Should().NotThrow();
        }
    }
}