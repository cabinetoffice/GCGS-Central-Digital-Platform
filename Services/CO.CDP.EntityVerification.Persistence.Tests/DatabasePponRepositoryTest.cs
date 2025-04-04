using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.EntityVerification.Persistence.IPponRepository.PponRepositoryException;
using static CO.CDP.EntityVerification.Persistence.Tests.PponFactories;

namespace CO.CDP.EntityVerification.Persistence.Tests;

public class DatabasePponRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedPpon()
    {
        using var repository = PponRepository();
        var ppon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        repository.Save(ppon);

        var found = await repository.FindPponByPponIdAsync(ppon.IdentifierId);

        found.Should().NotBeNull();
        found.As<Ppon>().Id.Should().BePositive();
        found.As<Ppon>().IdentifierId.Should().Be(ppon.IdentifierId);
    }

    [Fact]
    public async Task ItFindsPponByIdentifierPpon()
    {
        using var repository = PponRepository();
        var ppon = GivenPpon(pponId: "dba3fb78c1f3401fa44774c0ad2ba6bc");
        var identifier = new Identifier()
        {
            IdentifierId = "GB123123123",
            Scheme = "GB-COH",
            Id = 0,
            LegalName = "Acme Ltd",
            Uri = new Uri("https://www.acme-org.com")
        };
        ppon.Identifiers = [identifier];

        repository.Save(ppon);

        var found = await repository.FindPponByIdentifierAsync(identifier.Scheme, identifier.IdentifierId);

        found.Should().NotBeNull();
        found.As<Ppon>().Identifiers.Should().ContainSingle();
        found.As<Ppon>().Identifiers.First().IdentifierId.Should().Be("GB123123123");
    }

    [Fact]
    public async Task ItFindsregistriesByCountryCode()
    {
        await using var context = postgreSql.EntityVerificationContext();
        using var repository = PponRepository(context);

        var identifierRegistries = new List<IdentifierRegistries>
        {
            new()
            {
                CountryCode = "US",
                Scheme = "Scheme1",
                RegisterName = "Registry1",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            new()
            {
                CountryCode = "US",
                Scheme = "Scheme2",
                RegisterName = "Registry2",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            new IdentifierRegistries
            {
                CountryCode = "CA",
                Scheme = "Scheme3",
                RegisterName = "Registry3",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            }
        }.AsQueryable();

        await context.AddRangeAsync(identifierRegistries);
        await context.SaveChangesAsync();

        var found = await repository.GetIdentifierRegistriesAsync("US");

        found.Should().NotBeNull();
        found.Should().HaveCount(2);
        found.Should().OnlyContain(r => r.CountryCode == "US");
        found.Select(r => r.Scheme).Should().BeEquivalentTo(new[] { "Scheme1", "Scheme2" });
    }

    [Fact]
    public async Task ItFindsregistriesBySchemeCodes()
    {
        await using var context = postgreSql.EntityVerificationContext();
        using var repository = PponRepository(context);

        var schemeCodes = new[] { "SCHEME4", "SCHEME5" };

        var identifierRegistries = new List<IdentifierRegistries>
        {
            new IdentifierRegistries
            {
                CountryCode = "FR",
                Scheme = "Scheme4",
                RegisterName = "FR Registry1",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            new IdentifierRegistries
            {
                CountryCode = "FR",
                Scheme = "Scheme5",
                RegisterName = "FR Registry2",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            new IdentifierRegistries
            {
                CountryCode = "CA",
                Scheme = "Scheme6",
                RegisterName = "CA Registry3",
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            }
        }.AsQueryable();

        await context.AddRangeAsync(identifierRegistries);
        await context.SaveChangesAsync();

        var found = await repository.GetIdentifierRegistriesNameAsync(schemeCodes);

        found.Should().NotBeNull();
        found.Should().HaveCount(2);
        found.Should().OnlyContain(r => r.CountryCode == "FR");
        found.Select(r => r.Scheme).Should().BeEquivalentTo(new[] { "Scheme4", "Scheme5" });
    }

    [Fact]
    public void ItRejectsAnAlreadyKnownPponId()
    {
        using var repository = PponRepository();
        var ppon = GivenPpon(pponId: "d73c9fef3c8549e2b3999fa9ec0a5aea");
        var anotherPpon = GivenPpon(pponId: "d73c9fef3c8549e2b3999fa9ec0a5aea");

        repository.Save(ppon);

        repository.Invoking(r => r.Save(anotherPpon))
            .Should().Throw<DuplicatePponException>();
    }

    [Fact]
    public async Task ItSavesPponAndAdditionalEntitiesInASingleTransaction()
    {
        await using var context = postgreSql.EntityVerificationContext();
        using var repository = PponRepository(context);

        var ppon = GivenPpon(pponId: "f53c02c216444e87a9364454b529f891", name: "ACME");
        await repository.SaveAsync(ppon, _ =>
        {
            ppon.Name = "Updated name";
            repository.Save(ppon);
            return Task.CompletedTask;
        });

        var foundPpon = await repository.FindPponByPponIdAsync(ppon.IdentifierId);

        foundPpon.Should().NotBeNull();
        foundPpon.As<Ppon>().Name.Should().Be("Updated name");
    }

    [Fact]
    public async Task ItRevertsTheTransactionIfSavingOfAdditionalEntitiesFails()
    {
        await using var context = postgreSql.EntityVerificationContext();
        var repository = PponRepository(context);

        var ppon = GivenPpon(pponId: "772bd0fa18d3448f883502afba8eb5e3", name: "ACME 2");
        var act = async () => await repository.SaveAsync(ppon, _ =>
        {
            ppon.Name = "Updated name 2";
            repository.Save(ppon);
            throw new Exception("Failed in transaction");
        });

        await act.Should().ThrowAsync<Exception>("Failed in transaction");

        var foundPpon = await repository.FindPponByPponIdAsync(ppon.IdentifierId);

        foundPpon.Should().BeNull();
    }

    private IPponRepository PponRepository(EntityVerificationContext? context = null)
    {
        return new DatabasePponRepository(context ?? postgreSql.EntityVerificationContext());
    }
}