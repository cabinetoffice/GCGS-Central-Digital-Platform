using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseAuthenticationKeyRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedAuthenticationKey()
    {
        using var repository = AuthenticationKeyRepository();
        var organisation = GivenOrganisation();
        var authenticationKey = GivenAuthenticationKey(organisation: organisation);
        await repository.Save(authenticationKey);

        var found = await repository.Find(authenticationKey.Key);
        found.As<AuthenticationKey>().Id.Should().BePositive();
    }

    [Fact]
    public async Task ItReturnsNullIfAuthenticationKeyIsNotFound()
    {
        using var repository = AuthenticationKeyRepository();

        var found = await repository.Find("key_not_in_database");

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItUpdatesAnExistingAuthenticationKey()
    {
        using var repository = AuthenticationKeyRepository();
        var organisation = GivenOrganisation();
        var authenticationKey = GivenAuthenticationKey(organisation: organisation);
        authenticationKey.Organisation = GivenOrganisation();

        await repository.Save(authenticationKey);

        var found = await repository.Find(authenticationKey.Key);
        found.As<AuthenticationKey>().OrganisationId.Should().Be(authenticationKey.Organisation.Id);
    }

    private static AuthenticationKey GivenAuthenticationKey(
        string name = "fts",
        string key = "api-key",
        Organisation? organisation = null,
        List<string>? scopes = null
    )
    {
        return new AuthenticationKey
        {
            Name = name,
            Key = key,
            Organisation = organisation,
            Scopes = scopes ?? []
        };
    }

    private IAuthenticationKeyRepository AuthenticationKeyRepository()
        => new DatabaseAuthenticationKeyRepository(postgreSql.OrganisationInformationContext());
}