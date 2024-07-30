using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using static CO.CDP.Organisation.WebApi.UseCase.AssignIdentifierUseCase.AssignIdentifierException;

namespace CO.CDP.Organisation.WebApi.Events;

public class PponGeneratedSubscriber(
    IUseCase<AssignOrganisationIdentifier, bool> assignIdentifierUseCase,
    ILogger<PponGeneratedSubscriber> logger
) : ISubscriber<PponGenerated>
{
    public async Task Handle(PponGenerated @event)
    {
        try
        {
            await AssignIdentifier(@event);
        }
        catch (OrganisationNotFoundException cause)
        {
            logger.LogError(cause.Message, cause);
        }
    }

    private async Task AssignIdentifier(PponGenerated @event)
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