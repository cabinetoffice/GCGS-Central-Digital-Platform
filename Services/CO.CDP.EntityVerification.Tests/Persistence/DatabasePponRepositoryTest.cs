using CO.CDP.EntityVerification.Persistence;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.EntityVerification.Persistence.IPponRepository.PponRepositoryException;

namespace CO.CDP.EntityVerification.Tests.Persistence;

public class DatabasePponRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedPpon()
    {
        using var repository = PponRepository();
        var ppon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");

        repository.Save(ppon);

        var found = await FindPponByPponId(ppon.PponId);

        found.Should().NotBeNull();
        found.As<Ppon>().Id.Should().BePositive();
        found.As<Ppon>().PponId.Should().Be(ppon.PponId);
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

    private static Ppon GivenPpon(string? pponId = null)
    {
        return new Ppon
        {
            PponId = pponId ?? Guid.NewGuid().ToString().Replace("-", "")
        };
    }

    private async Task<Ppon?> FindPponByPponId(string pponId)
    {
        return await postgreSql.EntityVerificationContext()
            .Ppons
            .FirstOrDefaultAsync(p => p.PponId == pponId);
    }

    private IPponRepository PponRepository()
    {
        return new DatabasePponRepository(postgreSql.EntityVerificationContext());
    }
}