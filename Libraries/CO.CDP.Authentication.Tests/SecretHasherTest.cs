using FluentAssertions;

namespace CO.CDP.Authentication.Tests;

public class SecretHasherTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Hash_NullOrWhitespaceApiKey_ThrowsArgumentException(string secret)
    {
        Action act = () => SecretHasher.Hash(secret);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Hash_ShouldHashTheSecret()
    {
        var hashedValue = SecretHasher.Hash("A_VERY_SUPER_SECRET");

        hashedValue.Should().Be("F65F1D621CBADDDD6107941928A19D53E0869DE52DD202823AD82BE242839070");
    }
}