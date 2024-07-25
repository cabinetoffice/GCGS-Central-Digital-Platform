using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class DeleteAnswerSetUseCase(IFormRepository formRepository)
    : IUseCase<(Guid organisationId, Guid answerSetId), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid answerSetId) command)
    {
        return await formRepository.DeleteAnswerSetAsync(command.organisationId, command.answerSetId);
    }
}