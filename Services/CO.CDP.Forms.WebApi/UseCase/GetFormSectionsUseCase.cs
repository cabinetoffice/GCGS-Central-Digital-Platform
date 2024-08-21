using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class GetFormSectionsUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<(Guid formId, Guid organisationId), FormSectionResponse?>
{
    public async Task<FormSectionResponse?> Execute((Guid formId, Guid organisationId) command)
    {
        var (formId, organisationId) = command;

        var summaries = await formRepository.GetFormSummaryAsync(formId, organisationId);

        return summaries.Any() ? new FormSectionResponse { FormSections = mapper.Map<List<FormSectionSummary>>(summaries) } : null;
    }
}