using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class GetFormSectionsUseCase(IFormRepository formRepository, IMapper mapper, ILogger<GetFormSectionsUseCase> logger)
    : IUseCase<(Guid formId, Guid organisationId), FormSectionResponse?>
{
    public async Task<FormSectionResponse?> Execute((Guid formId, Guid organisationId) command)
    {
        var (formId, organisationId) = command;

        logger.LogInformation("GetFormSectionsUseCase - FormId: {FormId}, OrganisationId: {OrganisationId}", formId, organisationId);
        
        var summaries = await formRepository.GetFormSummaryAsync(formId, organisationId);
        
        logger.LogInformation("GetFormSectionsUseCase - Raw summaries count from repository: {Count}", summaries.Count());
        
        foreach (var summary in summaries)
        {
            logger.LogInformation("GetFormSectionsUseCase - Raw summary: SectionId: {SectionId}, SectionName: {SectionName}, Type: {Type} ({TypeValue}), AnswerSetCount: {AnswerSetCount}", 
                summary.SectionId, summary.SectionName, summary.Type, (int)summary.Type, summary.AnswerSetCount);
        }
        
        if (!summaries.Any())
        {
            logger.LogWarning("GetFormSectionsUseCase - No form summaries found for FormId: {FormId}, OrganisationId: {OrganisationId}", formId, organisationId);
            return null;
        }
        
        var mappedSections = mapper.Map<List<FormSectionSummary>>(summaries);
        
        logger.LogInformation("GetFormSectionsUseCase - Mapped sections count: {Count}", mappedSections.Count);
        
        foreach (var section in mappedSections)
        {
            logger.LogInformation("GetFormSectionsUseCase - Mapped section: SectionId: {SectionId}, Name: {SectionName}, Type: {Type} ({TypeValue}), AnswerSetCount: {AnswerSetCount}", 
                section.SectionId, section.SectionName, section.Type, (int)section.Type, section.AnswerSetCount);
        }

        return new FormSectionResponse { FormSections = mappedSections };
    }
}