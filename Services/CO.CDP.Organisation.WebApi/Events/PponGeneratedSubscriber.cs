using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;

namespace CO.CDP.Organisation.WebApi.Events;

public class PponGeneratedSubscriber(IUseCase<AssignOrganisationIdentifier, bool> assignIdentifierUseCase)
    : ISubscriber<PponGenerated>
{
    public async Task Handle(PponGenerated @event)
    {
        await assignIdentifierUseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = @event.OrganisationId,
            Identifier = new OrganisationIdentifier
            {
                Id = @event.Id,
                Scheme = @event.Scheme,
                LegalName = @event.LegalName
            }
        });
    }
}