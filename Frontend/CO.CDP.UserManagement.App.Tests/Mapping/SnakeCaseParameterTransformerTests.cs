using CO.CDP.UserManagement.App;
using FluentAssertions;

namespace CO.CDP.UserManagement.App.Tests.Mapping;

public class SnakeCaseParameterTransformerTests
{
    [Fact]
    public void TransformOutbound_WhenNull_ReturnsNull()
    {
        var transformer = new SnakeCaseParameterTransformer();

        transformer.TransformOutbound(null).Should().BeNull();
    }

    [Fact]
    public void TransformOutbound_WhenEmpty_ReturnsNull()
    {
        var transformer = new SnakeCaseParameterTransformer();

        transformer.TransformOutbound(string.Empty).Should().BeNull();
    }

    [Theory]
    [InlineData("CdpOrganisationId", "cdp_organisation_id")]
    [InlineData("AlreadySnake", "already_snake")]
    [InlineData("Name", "name")]
    public void TransformOutbound_ConvertsToSnakeCase(string input, string expected)
    {
        var transformer = new SnakeCaseParameterTransformer();

        transformer.TransformOutbound(input).Should().Be(expected);
    }
}
