using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.UseCase;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.Tests.UseCase;

public class LookupIdentifierUseCaseTest
{
    private LookupIdentifierUseCase UseCase => new();

    [Fact]
    public async Task Execute_IfIdentifierFound_ReturnsAllIdentifiers()
    {
        string testPponIdentifier = "CDP-PPON:ac73982be54e456c888d495b6c2c997f";
        LookupIdentifierQuery query = new LookupIdentifierQuery(testPponIdentifier);
        var foundRecord = await UseCase.Execute(query);

        foundRecord.Should().BeEquivalentTo(GivenPersistenceOrganisationInfo(testPponIdentifier), options => options.ComparingByMembers<IEnumerable<Model.Identifier?>>());
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

    private IEnumerable<Model.Identifier?> GivenPersistenceOrganisationInfo(string identifier)
    {
        return new List<Model.Identifier?>
        {
            new Model.Identifier
            {
                Id = "ac73982be54e456c888d495b6c2c997f",
                LegalName = "Acme",
                Scheme = "CDP-PPON",
                Uri = new Uri("https://www.acme-ltd.com")
            },
            new Model.Identifier
            {
                Id = "12345678",
                LegalName = "Acme",
                Scheme = "GB-COH",
                Uri = new Uri("https://www.acme-ltd.com")
            }
        };
    }
}