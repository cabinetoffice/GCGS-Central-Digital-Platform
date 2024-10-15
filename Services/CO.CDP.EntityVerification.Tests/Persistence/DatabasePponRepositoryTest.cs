using CO.CDP.EntityVerification.Persistence;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.EntityVerification.Persistence.IPponRepository.PponRepositoryException;
using static CO.CDP.EntityVerification.Tests.Ppon.PponFactories;

namespace CO.CDP.EntityVerification.Tests.Persistence;

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
        found.As<EntityVerification.Persistence.Ppon>().Id.Should().BePositive();
        found.As<EntityVerification.Persistence.Ppon>().IdentifierId.Should().Be(ppon.IdentifierId);
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
        found.As<EntityVerification.Persistence.Ppon>().Identifiers.Should().ContainSingle();
        found.As<EntityVerification.Persistence.Ppon>().Identifiers.First().IdentifierId.Should().Be("GB123123123");
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
        foundPpon.As<EntityVerification.Persistence.Ppon>().Name.Should().Be("Updated name");
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