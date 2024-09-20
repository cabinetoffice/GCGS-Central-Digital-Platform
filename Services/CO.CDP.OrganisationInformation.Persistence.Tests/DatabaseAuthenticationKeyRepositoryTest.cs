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
        var authenticationKey = GivenAuthenticationKey(key: Guid.NewGuid().ToString(), organisation: organisation);
        await repository.Save(authenticationKey);

        var found = await repository.Find(authenticationKey.Key);
        found.As<AuthenticationKey>().Id.Should().BePositive();
        found.As<AuthenticationKey>().OrganisationId.Should().BePositive();
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
        var authenticationKey = GivenAuthenticationKey(key: Guid.NewGuid().ToString(), organisation: organisation);
        authenticationKey.Organisation = GivenOrganisation();

        await repository.Save(authenticationKey);

        var found = await repository.Find(authenticationKey.Key);
        found.As<AuthenticationKey>().OrganisationId.Should().Be(authenticationKey.Organisation.Id);
    }

    [Fact]
    public async Task ItFindsSavedAuthenticationKeys()
    {
        using var repository = AuthenticationKeyRepository();
        var organisation = GivenOrganisation();
        var authenticationKey = GivenAuthenticationKey(key: Guid.NewGuid().ToString(), organisation: organisation);
        await repository.Save(authenticationKey);

        var found = await repository.GetAuthenticationKeys(organisation.Guid);
        found.As<IEnumerable<AuthenticationKey>>().Should().NotBeEmpty();
        found.As<IEnumerable<AuthenticationKey>>().Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ItFindsSavedAuthenticationKeyByNameKeyAndOrganisationId()
    {
        using var repository = AuthenticationKeyRepository();
        var organisation = GivenOrganisation();
        var key = Guid.NewGuid().ToString();
        var authenticationKey = GivenAuthenticationKey(key: key, organisation: organisation);
        await repository.Save(authenticationKey);

        var found = await repository.FindByKeyNameAndOrganisationId("fts", organisation.Guid);
        found.As<AuthenticationKey>().Should().NotBeNull();
        found.As<AuthenticationKey>().Key.Should().BeSameAs(key);
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