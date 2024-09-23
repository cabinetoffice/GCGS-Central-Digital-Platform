using CO.CDP.DataSharing.WebApi;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationInformation.Tests;
public class EndpointUriBuildingTests
{
    private readonly Mock<IConfigurationService> _configurationService = new();
    private readonly IIdentifierSchemes _identifierSchemes;

    public EndpointUriBuildingTests()
    {
        _identifierSchemes = new IdentifierSchemes(_configurationService.Object);
    }

    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedNumericIds()
    {
        foreach (var key in _identifierSchemes.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = _identifierSchemes.GetRegistryUri(key, Random.Shared.Next(999999).ToString());
            };

            result.Should().NotThrow();
        }
    }

    [Fact]
    public void GetEndpointUri_ReturnsValidUris_WhenProvidedGuidIds()
    {
        foreach (var key in _identifierSchemes.SchemesToEndpointUris.Keys)
        {
            var result = () =>
            {
                var uri = _identifierSchemes.GetRegistryUri(key, Guid.NewGuid().ToString());
            };

            result.Should().NotThrow();
        }
    }
}