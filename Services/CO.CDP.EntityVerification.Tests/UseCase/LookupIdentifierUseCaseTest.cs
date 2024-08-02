using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Tests.Persistence;
using CO.CDP.EntityVerification.UseCase;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;
using static CO.CDP.EntityVerification.Tests.Ppon.PponFactories;
using CO.CDP.EntityVerification;

namespace CO.CDP.EntityVerification.Tests.UseCase;

public class LookupIdentifierUseCaseTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private readonly IPponRepository _repository = new DatabasePponRepository(postgreSql.EntityVerificationContext());
    private LookupIdentifierUseCase UseCase => new(_repository);

    [Fact]
    public async Task Execute_IfIdentifierFound_ReturnsAllIdentifiers()
    {
        var ppon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");
        ppon.Identifiers = EntityVerification.Persistence.Identifier.GetPersistenceIdentifiers(GivenPersistenceOrganisationInfo());

        _repository.Save(ppon);

        string testPponIdentifier = $"{ppon.Identifiers.First().Scheme}:{ppon.Identifiers.First().IdentifierId}";
        LookupIdentifierQuery query = new LookupIdentifierQuery(testPponIdentifier);
        var foundRecord = await UseCase.Execute(query);

        foundRecord.Should().BeEquivalentTo(GivenPersistenceOrganisationInfo(), options => options.ComparingByMembers<IEnumerable<Model.Identifier?>>());
    }

    [Fact]
    public async Task Execute_IfIdentifierNotFound_ReturnsNull()
    {
        var act = await UseCase.Execute(new LookupIdentifierQuery("UNKNOWN-SCHEME:12345"));
        act.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_InvalidIdentifier_ThrowsInvalidIdentifierFormatException()
    {
        Func<Task> act = async () => await UseCase.Execute(new LookupIdentifierQuery(""));

        await act.Should().ThrowAsync<InvalidIdentifierFormatException>().WithMessage("Both scheme and identifier are required in the format: scheme:identifier");
    }

    private IEnumerable<EntityVerification.Events.Identifier> GivenPersistenceOrganisationInfo()
    {
        return new List<EntityVerification.Events.Identifier>
        {
            new EntityVerification.Events.Identifier
            {
                Id = "ac73982be54e456c888d495b6c2c997f",
                LegalName = "Acme",
                Scheme = "CDP-PPON",
                Uri = new Uri("https://www.acme-ltd.com")
            },
            new EntityVerification.Events.Identifier
            {
                Id = "12345678",
                LegalName = "Acme",
                Scheme = "GB-COH",
                Uri = new Uri("https://www.acme-ltd.com")
            }
        };
    }
}