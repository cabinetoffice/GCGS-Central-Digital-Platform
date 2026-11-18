using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class ApiKeyHasherTest
{
    [Fact]
    public void Hash_ReturnsLowercaseSha256Hex()
    {
        // sha256("abc")
        ApiKeyHasher.Hash("abc")
            .Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }

    [Fact]
    public void Hash_IsDeterministicAndDiffersPerKey()
    {
        ApiKeyHasher.Hash("key-1").Should().Be(ApiKeyHasher.Hash("key-1"));
        ApiKeyHasher.Hash("key-1").Should().NotBe(ApiKeyHasher.Hash("key-2"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Hash_ThrowsForNullOrWhitespace(string? key)
    {
        var act = () => ApiKeyHasher.Hash(key!);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("abcdef-1234", "1234")]
    [InlineData("ab", "ab")]
    public void Hint_ReturnsLastFourCharacters(string key, string expected)
    {
        ApiKeyHasher.Hint(key).Should().Be(expected);
    }
}
