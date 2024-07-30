using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using static CO.CDP.Organisation.WebApi.UseCase.AssignIdentifierUseCase.AssignIdentifierException;

namespace CO.CDP.Organisation.WebApi.Tests.Events;

public class PponGeneratedSubscriberTest
{
    private readonly Mock<IUseCase<AssignOrganisationIdentifier, bool>> _useCase = new();
    private readonly Mock<ILogger<PponGeneratedSubscriber>> _logger = new();
    private PponGeneratedSubscriber Subscriber => new(_useCase.Object, _logger.Object);

    [Fact]
    public async Task ItDelegatesTheIdentifierAssignmentToTheUseCase()
    {
        await Subscriber.Handle(new PponGenerated
        {
            OrganisationId = Guid.Parse("36a98954-f9d3-4695-a29b-b8b97318f3ac"),
            Id = "c0777aeb968b4113a27d94e55b10c1b4",
            Scheme = "CDP-PPON",
            LegalName = "Acme Ltd"
        });

        _useCase.Verify(u => u.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = Guid.Parse("36a98954-f9d3-4695-a29b-b8b97318f3ac"),
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "CDP-PPON",
                LegalName = "Acme Ltd"
            }
        }));
    }

    [Fact]
    public async Task ItIgnoresTheOrganisationNotFoundException()
    {
        var organisationId = Guid.Parse("36a98954-f9d3-4695-a29b-b8b97318f3ac");
        var exception = new OrganisationNotFoundException(organisationId);

        _useCase.Setup(u => u.Execute(It.IsAny<AssignOrganisationIdentifier>()))
            .Throws(exception);

        await Subscriber.Handle(PponGenerated(organisationId: organisationId));

        _logger.Invocations.Count.Should().BePositive();
    }

    [Fact]
    public async Task ItIgnoresTheIdentifierAlreadyAssignedException()
    {
        var organisationId = Guid.Parse("36a98954-f9d3-4695-a29b-b8b97318f3ac");
        var exception = new IdentifierAlreadyAssigned(organisationId, new OrganisationIdentifier
        {
            Id = "123945", LegalName = "Acme Ltd", Scheme = "CDP-PPON"
        });

        _useCase.Setup(u => u.Execute(It.IsAny<AssignOrganisationIdentifier>()))
            .Throws(exception);

        await Subscriber.Handle(PponGenerated(
            organisationId: organisationId, id: "123945", legalName: "Acme Ltd", scheme: "CDP-PPON"
        ));

        _logger.Invocations.Count.Should().BePositive();
    }

    private static PponGenerated PponGenerated(
        Guid organisationId,
        string? id = null,
        string? scheme = null,
        string? legalName = null)
    {
        return new PponGenerated
        {
            OrganisationId = organisationId,
            Id = id ?? "c0777aeb968b4113a27d94e55b10c1b4",
            Scheme = scheme ?? "CDP-PPON",
            LegalName = legalName ?? "Acme Ltd"
        };
    }
}