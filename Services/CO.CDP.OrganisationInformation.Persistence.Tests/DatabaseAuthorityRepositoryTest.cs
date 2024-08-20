using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseAuthorityRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private IAuthorityRepository AuthorityRepository()
    {
        return new DatabaseAuthorityRepository(postgreSql.OrganisationInformationContext());
    }

    private static RefreshToken GivenRefreshToken(string tokenHash, bool revoked = false, bool expired = false)
    {
        return new RefreshToken
        {
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddDays(expired ? -1 : 1),
            Revoked = revoked
        };
    }

    [Fact]
    public async Task Find_ShouldReturnRefreshToken_WhenTokenHashExistsAndIsValid()
    {
        var repository = AuthorityRepository();

        var refreshToken = GivenRefreshToken("valid-token-hash");

        await repository.Save(refreshToken);

        var result = await repository.Find("valid-token-hash");

        result.Should().Be(refreshToken);
    }

    [Fact]
    public async Task Find_ShouldReturnNull_WhenTokenHashDoesNotExist()
    {
        var repository = AuthorityRepository();

        var result = await repository.Find("token-hash-not-exists");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Find_ShouldReturnNull_WhenTokenHashExpired()
    {
        var repository = AuthorityRepository();

        var refreshToken = GivenRefreshToken("expired-token-hash", expired: true);

        await repository.Save(refreshToken);

        var result = await repository.Find("expired-token-hash");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Find_ShouldReturnNull_WhenTokenHashRevoked()
    {
        var repository = AuthorityRepository();

        var refreshToken = GivenRefreshToken("revoked-token-hash", revoked: true);

        await repository.Save(refreshToken);

        var result = await repository.Find("revoked-token-hash");

        result.Should().BeNull();
    }
}