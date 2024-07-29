using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Authentication.Tests;

public class ApiKeyValidatorTest
{
    private readonly ApiKeyValidator _validator;
    private const string ValidApiKey = "valid-api-key";
    private const string InvalidApiKey = "invalid-api-key";
    private readonly Mock<IAuthenticationKeyRepository> _repository = new();

    public ApiKeyValidatorTest()
    {
        _validator = new ApiKeyValidator(_repository.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_NullOrWhitespaceApiKey_ThrowsArgumentException(string? apiKey)
    {
        Func<Task> act = async () => await _validator.Validate(apiKey);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Validate_ValidApiKey_ReturnsTrue()
    {
        _repository.Setup(r => r.Find(It.IsAny<string>()))
            .ReturnsAsync(new AuthenticationKey { Name = "FTS", Key = ValidApiKey, Scopes = ["ADMIN"] });

        var (valid, organisation, scopes) = await _validator.Validate(ValidApiKey);

        valid.Should().BeTrue();
        organisation.Should().BeNull();
        scopes.Should().Contain("ADMIN");
    }

    [Fact]
    public async Task Validate_ValidApiKeyWithOrganisation_ReturnsTrue()
    {
        _repository.Setup(r => r.Find(It.IsAny<string>()))
            .ReturnsAsync(new AuthenticationKey { Name = "FTS", Key = ValidApiKey, OrganisationId = 42, Scopes = ["ADMIN"] });

        var (valid, organisation, scopes) = await _validator.Validate(ValidApiKey);

        valid.Should().BeTrue();
        organisation.Should().Be(42);
        scopes.Should().Contain("ADMIN");
    }

    [Fact]
    public async Task Validate_InvalidApiKey_ReturnsFalse()
    {
        _repository.Setup(r => r.Find(It.IsAny<string>()))
            .ReturnsAsync((AuthenticationKey?)null);

        var (valid, organisation, scopes) = await _validator.Validate(InvalidApiKey);

        valid.Should().BeFalse();
        organisation.Should().BeNull();
        scopes.Should().BeEmpty();
    }
}