using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.Events;

public class PponGeneratedSubscriberTest
{
    private readonly Mock<IUseCase<AssignOrganisationIdentifier, bool>> _useCase = new();
    private PponGeneratedSubscriber Subscriber => new(_useCase.Object);

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
}