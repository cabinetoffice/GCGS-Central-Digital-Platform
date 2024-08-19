using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Tests.Persistence;
using CO.CDP.EntityVerification.UseCase;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.EntityVerification.Tests.Ppon.PponFactories;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.Tests.UseCase;

public class LookupIdentifierUseCaseTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private readonly IPponRepository _repository = new DatabasePponRepository(postgreSql.EntityVerificationContext());
    private LookupIdentifierUseCase UseCase => new(_repository);

    [Fact]
    public async Task Execute_IfPponIdentifierFound_ReturnsAllIdentifiers()
    {
        var ppon = GivenPpon(pponId: "93bfe534225a4de1a7531b69dac3afe3");
        List<EntityVerification.Events.Identifier> givenIdentifiers = new List<EntityVerification.Events.Identifier>(GivenEventOrganisationInfo());
        ppon.Identifiers = EntityVerification.Persistence.Identifier.GetPersistenceIdentifiers(givenIdentifiers);

        _repository.Save(ppon);

        string testPponIdentifier = $"{IdentifierSchemes.Ppon}:{ppon.IdentifierId}";
        LookupIdentifierQuery query = new LookupIdentifierQuery(testPponIdentifier);
        var foundRecord = await UseCase.Execute(query);

        givenIdentifiers.Add(GetPponAsIdentifier(ppon));
        foundRecord.Should().BeEquivalentTo(givenIdentifiers, options => options.ComparingByMembers<IEnumerable<Model.Identifier?>>());
    }

    [Fact]
    public async Task Execute_IfIdentifierFound_ReturnsAllIdentifiers()
    {
        var ppon = GivenPpon(pponId: "b69ffded365449f6aa4c340f5997fd2e");
        List<EntityVerification.Events.Identifier> givenIdentifiers = new List<EntityVerification.Events.Identifier>(GivenEventOrganisationInfo());
        ppon.Identifiers = EntityVerification.Persistence.Identifier.GetPersistenceIdentifiers(givenIdentifiers);

        _repository.Save(ppon);

        string testPponIdentifier = $"{ppon.Identifiers.First().Scheme}:{ppon.Identifiers.First().IdentifierId}";
        LookupIdentifierQuery query = new LookupIdentifierQuery(testPponIdentifier);
        var foundRecord = await UseCase.Execute(query);

        givenIdentifiers.Add(GetPponAsIdentifier(ppon));
        foundRecord.Should().BeEquivalentTo(givenIdentifiers, options => options.ComparingByMembers<IEnumerable<Model.Identifier?>>());
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

    private IEnumerable<EntityVerification.Events.Identifier> GivenEventOrganisationInfo()
    {
        return new List<EntityVerification.Events.Identifier>
        {
            new EntityVerification.Events.Identifier
            {
                Id = Guid.NewGuid().ToString(),
                LegalName = "Acme",
                Scheme = "GB-SIC",
                Uri = new Uri("https://www.acme-ltd.com")
            },
            new EntityVerification.Events.Identifier
            {
                Id = Guid.NewGuid().ToString(),
                LegalName = "Acme",
                Scheme = "GB-COH",
                Uri = new Uri("https://www.acme-ltd.com")
            }
        };
    }

    private EntityVerification.Events.Identifier GetPponAsIdentifier(EntityVerification.Persistence.Ppon ppon)
    {
        return new EntityVerification.Events.Identifier { Id = ppon.IdentifierId, LegalName = ppon.Name, Scheme = IdentifierSchemes.Ppon };
    }
}